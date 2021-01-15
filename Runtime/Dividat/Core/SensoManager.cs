using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dividat;
using System.IO;

///<summary>Senso Manager summarizes all access to the Senso platform. You do not normally need to access
///the hardware and service representations <code>Dividat.Hardware</code> and <code>Dividat.Software</code>.
///Direct access to these two core classes is not needed in most cases. SensoManager</summary>
namespace Dividat {

    public enum SimulatedKeyInputType {StepPlate, CenterOfGravity};

    public class SensoManager : MonoBehaviour, IPlayBehaviour
    { 
        //User Configuration of the Manager
        #region EditorProperties
        [Header("Behaviour Configuration")]
        [Tooltip("If force on all plates combined is lower than this value, it is considered that the player is jumping")]
        public float jumpForceThreshold = 0.05f;
        [Tooltip("The maximum time a jump may last in seconds. If the person lands before this the jump is cancelled, the OnJumpLanded event is triggered. Else, the jump is cancelled with the OnJumpCancelled event.")]
        public float maxJumpTime = 1f;  

        [Tooltip("If there is less weight on Senso than this threshold, it is considered that no person is present. If there is no person present for more than activityTimeout, then personPresent becomes false.")]
        public float playerPresenceForceThreshold = 0.05f;
        [Tooltip("How many seconds without input above playerPresenceForceThreshold before personPresent becomes false.")]
        public float activityTimeout = 10f;

		[Tooltip("Filter shakyness of center of gravity movement.")]
		[Range(0f, 0.95f)]
		public float CenterOfGravityFilterStrength = 0.2f;

        [Header("Advanced Configuration")]
        [Tooltip("Allows to configure varying SensoSetups. Leave empty to get default setting.")]
        public SensoHardwareConfiguration sensoHardwareConfiguration;
        #endregion EditorProperties
        
        //Public Properties, offering all convenience functions to get Senso Status.
        #region Properties

        public static SensoManager Instance
        {
            get
            {
                if (_instance == null )
                {
                    _instance = FindObjectOfType<SensoManager> ();
                    if (_instance == null )
                    {
                        GameObject obj = new GameObject ();
                        obj.name = "SensoManager";
                        _instance = obj.AddComponent<SensoManager> ();
                    }
                }
                return _instance;
            }
        }

        public bool logging = true;

        [Header("Unity Editor Settings / Simulator")]
        [Tooltip("If StepPlate, pressing the left/right/up/down/space key will trigger a step on the relative plate. If CenterOfGravity, a simulated point-sized player will move around on the Senso. Use the second setting for balancing games or absolute position. In most cases, use StepPlates however.")]
        public SimulatedKeyInputType keyInputType = SimulatedKeyInputType.StepPlate;
        public float centerOfGravitySpeed = 3f;
        public string saveFileName = @"DividatSaveGame.json";
        public int gameDuration = 1200000;
        [Tooltip("autoHelloOnStart performs an OnHello when the game starts. It is necessary to activate this or to manually execute the OnHello from the context menu to load save files in the Editor. This setting has no effect in builds.")]
        public bool autoHelloOnStart = true;

        [Header("Current Status (Visualization Only, do not Edit)")]
        [SerializeField]
        private bool _ready = false;
        public bool Ready {
            get { return _ready; }
        }
        public bool Suspended {
            get { return _suspended; }
        }
        [SerializeField]
        private bool _suspended = false;

        public bool Ended {
            get { return _ended; }
        }
        [SerializeField]
        private bool _ended = false;
        public bool jump = false;

        //How long has the player been off the plate/force below jump threshold?
        public float JumpTimer {
            get {
                if (jump == true){
                    return _jumpTimer;
                }
                else return 0f;
            } 
        }
        
        //If there is more force than playerPresentForceThreshold on the Senso 
        //or there is less force, but activity timeout has not yet expired, we 
        //consider the player present (true), else this is false.
        public bool PlayerPresent {
            get { return _playerPresent; } 
            set { _playerPresent = value; }
        }

        //The combined force applied to all plates summed up
        public float TotalForce {
            get {
                return _totalForce;
            }
        }

        ///This is equivalent to the player's calculated center of gravity. 
        //It is centered at zero, and ranges as far as the Dimensions/2 of the 
        //SensoHardwareConfiguration.  
        public Vector2 CenterOfGravity {
            get {
                return _cog;
            }
        }

        //The Settings object as received from Dividat Play. 
        public Settings Settings {
            get {
                return _settings;
            }
        }

        //The Memory blob as received from Dividat Play.
        public GenericGameSave Memory {
            get {
                return _memory;
            }
        }
        #endregion Properties

        //Senso Play Updates / Application Status
        public event DividatEvent OnSuspended;
        public event DividatEvent OnResumed;
        public event DividatEvent OnReady;

        //Senso Movement Updates
        public event DividatPositionEvent OnJumped;
        public event DividatPositionEvent OnJumpLanded;
        public event DividatEvent OnJumpCancelled;
        public event DividatToggleEvent OnPlayerPresenceChanged;
        public event DividatDirectionEvent OnStepDown;
        public event DividatDirectionEvent OnStepReleased;

        public delegate void DividatEvent();
        public delegate void DividatToggleEvent(bool status);
        public delegate void DividatPositionEvent(float x, float y);
        public delegate void DividatDirectionEvent(Direction dir);
        
        #region Private Variables
        //Private Config Vars
        private static SensoManager _instance;
        private Settings _settings;
        private GenericGameSave _memory;
        private Vector2 _sensoCenter;

        //Private movement-related vars
        [SerializeField]
        private Plate[] _plates;
        
        private float _totalForce = 0f;

        [SerializeField]
        private Vector2 _cog;
        private Vector2 _lastValidCog;

        [SerializeField]
        private Vector2 _simulatedCog = Vector2.positiveInfinity;
        
        private float _jumpTimer = float.MaxValue;
        private float lastActivity = float.MinValue;

        [SerializeField]
        private bool _playerPresent = false;
        private bool _playerPresentLast = false;
        #endregion Private Vars

        #region Senso Interactions
        /**
         * Get the plate state. This includes the state of simulated key input.
         */
        public Plate GetPlateState(Direction direction){
            return _plates[(int)direction];
        }

        /**
         * Returns true if plate is currently being stepped on.
         */
        public bool GetPlateActive(Direction direction)
        {
            return _plates[(int)direction].active;
        }

        /**
         * Returns true if plate was stepped on during the time of the last frame.
         */
        public bool GetStep(Direction dir){
            return _plates[(int)dir].changedAt == Time.frameCount-1 && _plates[(int)dir].active;
        }

        /**
         * Returns true step was released on plate during the time of the last frame.
         */
        public bool GetRelease(Direction dir){
            return _plates[(int)dir].changedAt == Time.frameCount-1 && !_plates[(int)dir].active;
        }

        public void Vibrate(MotorPattern preset){
            Hardware.ActivateMotor(preset);   
        }

        //public void Store(string serializedMemory)
        //{
        //    _memory = serializedMemory;
        //}
        #endregion


        #region Unity Boilerplate functions
        protected void Awake ()
        {
            //Check the singleton is unique
            if ( _instance == null )
            {
                _instance = SensoManager.Instance;
            }
            else
            {
                if (FindObjectsOfType<SensoManager>().Length > 1)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            //Set up the Hardware configuration
            if (_instance.sensoHardwareConfiguration == null)
            {
                _instance.sensoHardwareConfiguration = SensoHardwareConfiguration.InstantiateStandardConfiguration();
            }
            _instance._plates = new Plate[Instance.sensoHardwareConfiguration.hardwarePlates.Length];
            DontDestroyOnLoad(gameObject);
        }

        protected void Start(){
            // Register hooks with platform interface
            Debug.Log("[SensoManager] Start");
            Play.Init(this);
            if (autoHelloOnStart) {
                #if UNITY_EDITOR
                SimulatedOnHello();
                #endif
            }
            logging = Debug.isDebugBuild;
            _sensoCenter = sensoHardwareConfiguration.upperLeftCorner + sensoHardwareConfiguration.Dimensions/2f;
            _cog = _lastValidCog = _simulatedCog = _sensoCenter;
        }

        // Update is called once per frame
        protected void Update()
        {
            ProcessSensoInput();

            //Check Activity Timeouts and if Player Present
            if (_totalForce > playerPresenceForceThreshold){
                lastActivity = Time.time;
            }
            else {
                //if noone is there, reset the center of gravity
                _cog = _sensoCenter;
            }
            _playerPresent = Time.time - lastActivity < activityTimeout;
            if (_playerPresent != _playerPresentLast){
                OnPlayerPresenceChanged?.Invoke(_playerPresent);
            }

            //Check for movement patterns
            if (_totalForce < jumpForceThreshold){
                if (!jump && Mathf.Abs(_jumpTimer) < playerPresenceForceThreshold){
                    OnJumped?.Invoke(_cog.x, _cog.y);
                    if (logging) Debug.Log("Jumped");
                    _jumpTimer = 0f;
                    jump = true;
                }
                else {
                    _jumpTimer += Time.deltaTime;
                    if (_jumpTimer > maxJumpTime & jump){
                        jump = false;
                        if (logging) Debug.Log("Jump Cancelled");
                        OnJumpCancelled?.Invoke();
                    }
                }
            }
            else{
                if (jump) {
                    OnJumpLanded?.Invoke(_cog.x, _cog.y);
                    if (logging) Debug.Log("Jump landed");
                }
                jump=false;
                _jumpTimer = 0f;
            }
        }

        /// Read Input from Senso Hardware and Simulated Key Input, and calculate all the updated cog, plate states, etc.
        private void ProcessSensoInput(){
            _lastValidCog = _cog;
            //1. UPDATE PLATE STATE
            _plates = (Plate[])Hardware.Plates.Clone();
            
            Direction[] dirs = (Direction[])System.Enum.GetValues(typeof(Direction));
            foreach(Direction dir in dirs){
                if (GetStep(dir)){
                    OnStepDown?.Invoke(dir);
                }
                if (GetRelease(dir)){
                    OnStepReleased?.Invoke(dir);
                }
            }
            
            bool keyInputDetected = false;
            if (keyInputType == SimulatedKeyInputType.StepPlate){
                foreach(Direction dir in dirs){
                    KeyCode key = DirectionToKey(dir);
                    if (Input.GetKeyDown(key)){
                        //Debug.Log("step " + dir);
                        _plates[(int)dir] = GetSimulatedPlate(dir, true, true);
                        keyInputDetected = true;  
                    }
                    else if (Input.GetKeyUp(key)){
                        //Debug.Log("release " + dir);
                        _plates[(int)dir] = GetSimulatedPlate(dir, false, true);
                        keyInputDetected = true;
                    }
                    else if (Input.GetKey(key)){
                        //Debug.Log("down " + dir); 
                        _plates[(int)dir] = GetSimulatedPlate(dir, true, false); 
                        keyInputDetected=true;
                    }
                }
            }
            else if (keyInputType == SimulatedKeyInputType.CenterOfGravity) {
                foreach(Direction dir in dirs){
                    KeyCode key = DirectionToKey(dir);
                    if (Input.GetKey(key)){
                        keyInputDetected=true;
                        _simulatedCog += Time.deltaTime*centerOfGravitySpeed*DirectionToVector(dir);
                        if (_simulatedCog.x < sensoHardwareConfiguration.upperLeftCorner.x){
                             _simulatedCog.x = sensoHardwareConfiguration.upperLeftCorner.x;
                        }
                        else if (_simulatedCog.x > sensoHardwareConfiguration.lowerRightCorner.x){
                            _simulatedCog.x = sensoHardwareConfiguration.lowerRightCorner.x;
                        }
                        if (_simulatedCog.y < sensoHardwareConfiguration.upperLeftCorner.y){
                            _simulatedCog.y = sensoHardwareConfiguration.upperLeftCorner.y;
                        }
                        else if (_simulatedCog.y > sensoHardwareConfiguration.lowerRightCorner.y){
                            _simulatedCog.y = sensoHardwareConfiguration.lowerRightCorner.y;
                        }
                        SensoPlateSetup targetPlate = sensoHardwareConfiguration.GetCorrespondingDirection(_simulatedCog);
                        if (targetPlate != null)
                        {
                            _plates[(int)targetPlate.direction] = new Plate(_simulatedCog.x, _simulatedCog.y, 0.25f);
                        }
                    }
                }
            }

            //Calculate the center of gravity and the cumulated force on all plates
            Vector2 cog = new Vector2();
            float weight = 0f;
            for (int i=0; i < _plates.Length; i++)
            {
                cog += _plates[i].f*(new Vector2(_plates[i].x, _plates[i].y));
                weight += _plates[i].f;
            }
            if (Mathf.Abs(weight) > playerPresenceForceThreshold)
            {
				_cog = Vector3.Lerp(_lastValidCog, 1 / weight * cog - _sensoCenter, 1 - CenterOfGravityFilterStrength); //adjust so center is 0,0
            }
            else { 
                _cog = _lastValidCog;
            }
            _totalForce = weight;
        }
#endregion Unity Boilerplate

#region Process Senso Events
        public void Finish(Metrics metrics){
            Finish(metrics, _memory);
        }

        protected void Finish(Metrics metrics, GenericGameSave memory)
        {
            _ready = false;
            _ended = true;
            #if !UNITY_EDITOR
            Play.Finish(metrics, JsonUtility.ToJson(memory));
            #else
            File.WriteAllText(saveFileName, JsonUtility.ToJson(memory));
            #endif
        }

        /**
         *  Finish will cause Play to Terminate the application. 
         */
        public void Finish(Metrics metrics, PlaySaveGame memory){
            GenericGameSave save = GenericGameSave.Wrap(memory);
            Finish(metrics, save);
        }

        public void OnHello(Settings settings, string memory)
        {
            if (logging) Debug.Log("[SensoManager] OnHello");
            if (logging) Debug.Log("Settings: " +JsonUtility.ToJson(settings));
            if (logging) Debug.Log("Memory: " + memory);
            _settings = settings;
            GenericGameSave save = null;
            if (memory != null)
            {
                save = JsonUtility.FromJson<GenericGameSave>(memory);
            }
            _memory = save;
            _ready = true;
            OnReady?.Invoke();
            #if !UNITY_EDITOR
            Play.Ready();
            #endif
        }

        [ContextMenu("OnHello")]
        public void SimulatedOnHello(){
            Debug.Log("SimulatedOnHello");
            Settings s = new Settings();
            s.Add("duration", new Setting.Time(gameDuration));
            string saveGameJSON ="";
            if (File.Exists(saveFileName))
            {
                saveGameJSON = File.ReadAllText(saveFileName);
                if (logging) Debug.Log("read: " + saveGameJSON);
            }
            OnHello(s, saveGameJSON);
        }

        public void OnPing()
        {
            Play.Pong();
            if (logging) Debug.Log("[SensoManager] OnPing");   
        }

        public void OnSuspend()
        {
            _suspended = true;
            if (logging) Debug.Log("[SensoManager] OnSuspend. Subscribe to OnSuspended event for alerts.");
            OnSuspended?.Invoke();
        }

        public void OnResume()
        {
            _suspended = false;
            if (logging) Debug.Log("[SensoManager] OnResume");
            OnResumed?.Invoke();
        }
#endregion Process Senso Events

#region Simulation Support Functions
        private static KeyCode DirectionToKey(Direction direction)
        {
            switch (direction)
            {
                case Direction.Center:
                    return KeyCode.Space;
                case Direction.Up:
                    return KeyCode.UpArrow;
                case Direction.Right:
                    return KeyCode.RightArrow;
                case Direction.Down:
                    return KeyCode.DownArrow;
                case Direction.Left:
                    return KeyCode.LeftArrow;
                default:
                    throw new System.Exception("Exhaustiveness?");
            }
        }

        private static Vector2 DirectionToVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Center:
                    return Vector2.zero;
                case Direction.Up:
                    return Vector2.down;
                case Direction.Right:
                    return Vector2.right;
                case Direction.Down:
                    return Vector2.up;
                case Direction.Left:
                    return Vector2.left;
                default:
                    throw new System.Exception("Exhaustiveness?");
            }
        }

        /// Returns a simulated digital input on plate. Always positions the input at the center of the given plate.
        private Plate GetSimulatedPlate(Direction direction, bool active, bool changedThisFrame){ 
            switch (direction)
            {
                case Direction.Up:
                    return new Plate(1.5f, 0.5f, active ? 0.25f : 0f, active, changedThisFrame? Time.frameCount : _plates[(int)direction].changedAt);
                case Direction.Down:
                    return new Plate(1.5f, 2.5f, active ? 0.25f : 0f, active, changedThisFrame? Time.frameCount : _plates[(int)direction].changedAt);
                case Direction.Left:
                    return new Plate(0.5f, 1.5f, active ? 0.25f : 0f, active, changedThisFrame? Time.frameCount : _plates[(int)direction].changedAt);
                case Direction.Right:
                    return new Plate(2.5f, 1.5f, active ? 0.25f : 0f, active, changedThisFrame? Time.frameCount : _plates[(int)direction].changedAt);
                case Direction.Center:
                default:
                    return new Plate(1.5f, 1.5f, active ? 0.25f : 0f, active, changedThisFrame? Time.frameCount : _plates[(int)direction].changedAt);
            }
        }

#endregion
    }
}
