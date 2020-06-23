using System.Collections;
using System.Collections.Generic;
using Dividat;
using UnityEngine;

///<summary>Senso Manager summarizes all access to the Senso platform. You do not normally need to access
///the hardware and service representations <code>Dividat.Hardware</code> and <code>Dividat.Software</code>.
///You should never need direct access to the two plugin classes, as they do not support simulated key input.
///Use SensoManager.Instance to instantiate a singleton.</summary>
public class SensoManager : MonoBehaviour, IPlayBehaviour
{ 
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

                    //Set up the Hardware configuration
                    if (_instance.sensoHardwareConfiguration == null){
                         _instance.SetupDefaultConfiguration();
                    }
                    _instance._plates = new Plate[Instance.sensoHardwareConfiguration.hardwarePlates.Length];
                    
				}
			}
			return _instance;
		}
    }
    
    public SensoHardwareConfiguration sensoHardwareConfiguration;
    public float jumpForceThreshold = 0.05f;
    public float maxJumpTime = 2f;
    public bool jump = false;
    public bool playerPresent = false;
    public float activityTimeout = 10f; //How many seconds without input before personPresent becomes true
    public Settings Settings {
        get {
            return _settings;
        }
    }
    
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

    public float TotalForce {
        get {
            return _totalForce;
        }
    }

    //Private Status Vars
    private Plate[] _plates;
    private bool logging = false;
    public delegate void DividatEvent();
    public delegate void DividatPositionEvent(float x, float y);
    private Settings _settings;
    private float _totalForce = 0f;
    private static SensoManager _instance;
    private Vector2 _cog;
    private Vector2 _lastValidCog;
    private Vector2 _sensoCenter;
    private float _jumpTimer = float.MaxValue;

    private float lastActivity = float.MinValue;

    protected virtual void Awake ()
	{
        //Check the singleton is unique
		if ( _instance == null )
		{
			_instance = this;
			DontDestroyOnLoad ( gameObject );
		}
		else
		{
			Destroy ( gameObject );
		}

        
	}

    void Start(){
        // Register hooks with platform interface
        Play.Init(this);
        logging = Debug.isDebugBuild;
        _sensoCenter = sensoHardwareConfiguration.upperLeftCorner + sensoHardwareConfiguration.Dimensions/2f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //1. UPDATE PLATE STATE (including simulation with keys)
        _plates = (Plate[])Hardware.Plates.Clone();
        
        Direction[] dirs = (Direction[])System.Enum.GetValues(typeof(Direction));
        foreach(Direction dir in dirs){
            KeyCode key = DirectionToKey(dir);
            if (Input.GetKeyDown(key)){
                //Debug.Log("step " + dir);
                _plates[(int)dir] = GetSimulatedPlate(dir, true, true);
                
            }
            else if (Input.GetKeyUp(key)){
                //Debug.Log("release " + dir);
                _plates[(int)dir] = GetSimulatedPlate(dir, false, true);
                
            }
            else if (Input.GetKey(key)){
                //Debug.Log("down " + dir); 
                _plates[(int)dir] = GetSimulatedPlate(dir, true, false); 
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
        if (Mathf.Abs(weight) > 0.01f)
        {
            _cog = 1 / weight * cog;
        }
        else {
            _cog = _sensoCenter;
        }
        _totalForce = weight;

        //Check Activity Timeouts and if Player Present
        if (_totalForce > 0.01f){
            lastActivity = Time.time;
        }
        playerPresent = Time.time - lastActivity < activityTimeout;

        //Check for movement patterns
        if (_totalForce < jumpForceThreshold){
            if (!jump && Mathf.Abs(_jumpTimer) < 0.01f){
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

    
    public void SetupDefaultConfiguration(){
        sensoHardwareConfiguration = ScriptableObject.CreateInstance<SensoHardwareConfiguration>();
        sensoHardwareConfiguration.upperLeftCorner = new Vector2(0f, 0f);
        sensoHardwareConfiguration.lowerRightCorner = new Vector2(3f, 3f);
        sensoHardwareConfiguration.hardwarePlates = new SensoPlateSetup[5];

        //center
        sensoHardwareConfiguration.hardwarePlates[0] = new SensoPlateSetup();
        sensoHardwareConfiguration.hardwarePlates[0].direction = Direction.Center;
        sensoHardwareConfiguration.hardwarePlates[0].upperLeftCorner = new Vector2(1f, 1f);
        sensoHardwareConfiguration.hardwarePlates[0].lowerRightCorner = new Vector2(2f, 2f);

        //up
        sensoHardwareConfiguration.hardwarePlates[1] = new SensoPlateSetup();
        sensoHardwareConfiguration.hardwarePlates[1].direction = Direction.Up;
        sensoHardwareConfiguration.hardwarePlates[1].upperLeftCorner = new Vector2(0f, 0f);
        sensoHardwareConfiguration.hardwarePlates[1].lowerRightCorner = new Vector2(3f, 1f);

        //right
        sensoHardwareConfiguration.hardwarePlates[2] = new SensoPlateSetup();
        sensoHardwareConfiguration.hardwarePlates[2].direction = Direction.Right;
        sensoHardwareConfiguration.hardwarePlates[2].upperLeftCorner = new Vector2(2f, 0f);
        sensoHardwareConfiguration.hardwarePlates[2].lowerRightCorner = new Vector2(3f, 3f);

        //down
        sensoHardwareConfiguration.hardwarePlates[3] = new SensoPlateSetup();
        sensoHardwareConfiguration.hardwarePlates[3].direction = Direction.Down;
        sensoHardwareConfiguration.hardwarePlates[3].upperLeftCorner = new Vector2(0f, 2f);
        sensoHardwareConfiguration.hardwarePlates[3].lowerRightCorner = new Vector2(3f, 3f);

        //left
        sensoHardwareConfiguration.hardwarePlates[4] = new SensoPlateSetup();
        sensoHardwareConfiguration.hardwarePlates[4].direction = Direction.Down;
        sensoHardwareConfiguration.hardwarePlates[4].upperLeftCorner = new Vector2(0f, 0f);
        sensoHardwareConfiguration.hardwarePlates[4].lowerRightCorner = new Vector2(1f, 3f);
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
