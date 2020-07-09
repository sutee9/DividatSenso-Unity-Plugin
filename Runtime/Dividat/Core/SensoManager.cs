using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dividat;

///<summary>Senso Manager summarizes all access to the Senso platform. You do not normally need to access
///the hardware and service representations <code>Dividat.Hardware</code> and <code>Dividat.Software</code>.
///Direct access to these two core classes is not needed in most cases. SensoManager</summary>
namespace Dividat {

    public enum SimulatedKeyInputType {StepPlate, CenterOfGravity};

    public class SensoManager : MonoBehaviour, IPlayBehaviour
    { 
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

        [Header("Key Input Simulation")]
        [Tooltip("If StepPlate, pressing the left/right/up/down/space key will trigger a step on the relative plate. If CenterOfGravity, a simulated point-sized player will move around on the Senso. Use the second setting for balancing games or absolute position. In most cases, use StepPlates however.")]
        public SimulatedKeyInputType keyInputType = SimulatedKeyInputType.StepPlate;

        public float centerOfGravitySpeed = 3f;

        [Header("Current Status")]
        public bool jump = false;
        public bool playerPresent = false;
        #endregion EditorProperties

        [Header("Advanced Configuration")]
        [Tooltip("Allows to configure varying SensoSetups. Leave empty to get default setting.")]
        public SensoHardwareConfiguration sensoHardwareConfiguration;

        
        //Public Properties
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
        

        public float TotalForce {
            get {
                return _totalForce;
            }
        }
       


 
        
        public Settings Settings {
            get {
                return _settings;
            }
        }
        
        public delegate void DividatEvent();
        public delegate void DividatPositionEvent(float x, float y);
        public event DividatEvent OnSuspended;
        public event DividatEvent OnResumed;
        public event DividatEvent OnReady;

        public event DividatPositionEvent OnJumped;
        public event DividatPositionEvent OnJumpLanded;
        public event DividatEvent OnJumpCancelled;

        ///This is equivalent to the player's calculated center of gravity.
        public Vector2 CenterOfGravity {
            get {
                return _cog;
            }
        }

        #region Private Variables
        //Private Config Vars
        private static SensoManager _instance;
        private Settings _settings;
        private Vector2 _sensoCenter;

        //Private movement-related vars
        [SerializeField]
        private Plate[] _plates;
        
        private bool logging = false;
        private float _totalForce = 0f;
        
        private Vector2 _cog;
        private Vector2 _lastValidCog;

        [SerializeField]
        private Vector2 _simulatedCog = Vector2.positiveInfinity;
        
        private float _jumpTimer = float.MaxValue;
        private float lastActivity = float.MinValue;
        #endregion Private Vars

        protected void Awake ()
        {
            //Check the singleton is unique
            if ( _instance == null )
            {
                Debug.Log("Hello");
                _instance = SensoManager.Instance;

                //Set up the Hardware configuration
                if (_instance.sensoHardwareConfiguration == null){
                    _instance.sensoHardwareConfiguration = SensoHardwareConfiguration.InstantiateStandardConfiguration();
                }
                _instance._plates = new Plate[Instance.sensoHardwareConfiguration.hardwarePlates.Length];
                DontDestroyOnLoad ( gameObject );
            }
            else
            {
                Destroy ( gameObject );
            }
        }

        protected void Start(){
            // Register hooks with platform interface
            Play.Init(this);
            logging = Debug.isDebugBuild;
            _sensoCenter = sensoHardwareConfiguration.upperLeftCorner + sensoHardwareConfiguration.Dimensions/2f;
            _cog = _lastValidCog = _simulatedCog = _sensoCenter;
        }

        // Update is called once per frame
        protected void LateUpdate()
        {
            _lastValidCog = _cog;
            //1. UPDATE PLATE STATE
            _plates = (Plate[])Hardware.Plates.Clone();
            
            Direction[] dirs = (Direction[])System.Enum.GetValues(typeof(Direction));
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
                        if (targetPlate != null){
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
                _cog = 1 / weight * cog;
            }
            else {
                _cog = _lastValidCog;
            }
            _totalForce = weight;

            //Check Activity Timeouts and if Player Present
            if (_totalForce > 0.01f){
                lastActivity = Time.time;
            }
            playerPresent = Time.time - lastActivity < activityTimeout;

            //Check for movement patterns
            if (_totalForce < jumpForceThreshold){
                if (!jump && Mathf.Abs(_jumpTimer) < playerPresenceForceThreshold){
                    OnJumped?.Invoke(_cog.x, _cog.y);
                    Debug.Log("Jumped");
                    _jumpTimer = 0f;
                    jump = true;
                }
                else {
                    _jumpTimer += Time.deltaTime;
                    if (_jumpTimer > maxJumpTime & jump){
                        jump = false;
                        Debug.Log("Jump Cancelled");
                        OnJumpCancelled?.Invoke();
                    }
                }
            }
            else{
                if (jump) {
                    OnJumpLanded?.Invoke(_cog.x, _cog.y);
                    Debug.Log("Jump landed");
                }
                jump=false;
                _jumpTimer = 0f;
            }
        }

        public void OnHello(Settings settings)
        {
            if (logging) Debug.Log("[SensoManager] OnHello");
            Debug.Log(settings.ToString());
            _settings = settings;
            Play.Ready();
            OnReady?.Invoke();
        }

        public void OnPing()
        {
            Play.Pong();
            if (logging) Debug.Log("[SensoManager] OnPing");   
        }

        public void OnResume()
        {
            if (logging) Debug.Log("[SensoManager] OnResume");
            OnResumed?.Invoke();
        }

        public void OnSuspend()
        {
            if (logging) Debug.Log("[SensoManager] OnSuspend. Subscribe to OnSuspended event for alerts.");
            OnSuspended?.Invoke();
        }
        
        /**
        * Get the plate state. This includes the state of simulated key input.
        */
        public Plate GetPlateState(Direction direction){
            return _plates[(int)direction];
        }

        public bool GetPlateActive(Direction direction)
        {
            return _plates[(int)direction].active;
        }

        public bool GetStep(Direction dir){
            return _plates[(int)dir].changedAt == Time.frameCount-1 && _plates[(int)dir].active;
        }

        public bool GetRelease(Direction dir){
            return _plates[(int)dir].changedAt == Time.frameCount-1 && !_plates[(int)dir].active;
        }
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
    }
}
