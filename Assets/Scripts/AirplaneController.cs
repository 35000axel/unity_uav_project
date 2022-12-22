using UnityEngine;
using System.Collections;

public class AirplaneController : MonoBehaviour 
{
    //set to false for use in other script
    public bool userControl = true;

    //The propellers
    GameObject propellerFR;
    GameObject propellerFL;
    GameObject propellerBL;
    GameObject propellerBR;

    //Quadcopter parameters
    [Header("Internal")]
    public float maxTorque = 1f;
    public float throttle;
    public float moveFactor = 5f; 

    float maxPropellerForce;
    
    //PID
    public Vector3 PID_pitch_gains = new Vector3(2,3,2); 
    public Vector3 PID_roll_gains = new Vector3(2,3,2);   
    public Vector3 PID_yaw_gains = new Vector3(1,0,0);

    public Vector3 PID_altitude_gains = new Vector3(100f,60f,100f);

    //External parameters
    [Header("External")]
    public float windForce;
    //0 -> 360
    public float forceDir;

    Rigidbody quadcopterRB;

    //The PID controllers
    private PIDController PID_pitch;
    private PIDController PID_roll;
    private PIDController PID_yaw;
    private PIDController PID_altitude;

    //Movement factors
    [HideInInspector]
    public float moveForwardBack = 0f;
    [HideInInspector]
    public float moveLeftRight = 0f;
    [HideInInspector]
    public float yawDir = 0f;

    float gravityforce;

    float targetAltitude;

    float xAngle = 0f;
    float zAngle = 0f;

    float maxAngle = 45f;

    float dragCoef = 7f;

    void Start() 
	{
        quadcopterRB = gameObject.GetComponent<Rigidbody>();

        PID_pitch = new PIDController();
        PID_roll = new PIDController();
        PID_yaw = new PIDController();
        PID_altitude = new PIDController();

        gravityforce = 9.81f * 5f /4f;
        throttle = gravityforce;

        targetAltitude = transform.position.y ;
        
        quadcopterRB.drag = dragCoef;

        maxPropellerForce = gravityforce * 5f;

        propellerFR = transform.Find("Motor_FR").gameObject;
        propellerFL = transform.Find("Motor_FL").gameObject;
        propellerBL = transform.Find("Motor_BL").gameObject;
        propellerBR = transform.Find("Motor_BR").gameObject;
    }

    void Update(){

        if (userControl){
            AddControls();
            if(Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)){
                targetAltitude = transform.position.y;
            }
        }

    }

    void FixedUpdate()
    {

        AddMotorForce();

        // AddExternalForces();
    }

    void AddControls()
    {
        moveForwardBack = 0f;

        if (Input.GetKey(KeyCode.Z))
        {
            moveForwardBack = 5f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveForwardBack = -5f;
        }

        moveLeftRight = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            moveLeftRight = -5f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveLeftRight = 5f;
        }

        yawDir = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            yawDir = -5f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            yawDir = 5f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            throttle += 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            throttle -= 1f;            
        }

        throttle = Mathf.Clamp(throttle, 0f, maxPropellerForce );
        
    }

    bool diagonalLimitsAreExceed(){
        Vector3 pointB = transform.position;
        //difference between propeller gravity center and core gravity center when quadricopter all angles are 0
        float adjust = 0.125f;

        Vector3 FR_pointA = propellerFR.transform.position - new Vector3(0,adjust,0);
        Vector3 FR_pointC = new Vector3(FR_pointA.x , pointB.y, FR_pointA.z);

        Vector3 FL_pointA = propellerFL.transform.position - new Vector3(0,adjust,0);
        Vector3 FL_pointC = new Vector3(FL_pointA.x , pointB.y, FL_pointA.z);

        Vector3 BL_pointA = propellerBL.transform.position - new Vector3(0,adjust,0);
        Vector3 BL_pointC = new Vector3(BL_pointA.x , pointB.y, BL_pointA.z);

        Vector3 BR_pointA = propellerBR.transform.position - new Vector3(0,adjust,0);
        Vector3 BR_pointC = new Vector3(BR_pointA.x , pointB.y, BR_pointA.z);


        float propellerFR_diago_angle = Vector3.Angle(pointB - FR_pointA, FR_pointC - pointB) - 180;
        float propellerFL_diago_angle = Vector3.Angle(pointB - FL_pointA, FL_pointC - pointB) - 180;
        float propellerBL_diago_angle = Vector3.Angle(pointB - BL_pointA, BL_pointC - pointB) - 180;
        float propellerBR_diago_angle = Vector3.Angle(pointB - BR_pointA, BR_pointC - pointB) - 180;

        return propellerFR_diago_angle > maxAngle || propellerFL_diago_angle > maxAngle || propellerBL_diago_angle > maxAngle || propellerBR_diago_angle > maxAngle || propellerFR_diago_angle < -maxAngle || propellerFL_diago_angle < -maxAngle || propellerBL_diago_angle < -maxAngle || propellerBR_diago_angle < -maxAngle;
    }

    void AddMotorForce()
    {
        //Returns positive if pitching forward
        float pitchError = GetPitchError();

        //Returns positive if rolling left
        float rollError = GetRollError() * -1f;

        float altitudeError = GetAltitudeError();

        //Get the output from the PID controllers
        float PID_pitch_output = PID_pitch.GetFactorFromPIDController(PID_pitch_gains, pitchError, Time.fixedDeltaTime);
        float PID_roll_output = PID_roll.GetFactorFromPIDController(PID_roll_gains, rollError, Time.fixedDeltaTime);
        float PID_altitude_output = PID_altitude.GetFactorFromPIDController(PID_altitude_gains, altitudeError, Time.fixedDeltaTime);       
        
        if(!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)){
            throttle += PID_altitude_output;
        }

        //diagonals angles
        bool diagoAngleExceed = diagonalLimitsAreExceed();

        if(moveForwardBack != 0 ){
            if(xAngle<maxAngle && xAngle>-maxAngle && !diagoAngleExceed){
                PID_pitch_output = 0;
            }
        }

        if(moveLeftRight != 0 ){
            if(zAngle<maxAngle && zAngle>-maxAngle && !diagoAngleExceed){
                PID_roll_output = 0;
            }
        }
        
        throttle = Mathf.Clamp(throttle, 0f, maxPropellerForce);

        //Calculate the propeller forces
        //FR
        float propellerForceFR = throttle + (PID_pitch_output + PID_roll_output);
        propellerForceFR -= moveForwardBack * throttle * moveFactor;
        propellerForceFR -= moveLeftRight * throttle;


        //FL
        float propellerForceFL = throttle + (PID_pitch_output - PID_roll_output);
        propellerForceFL -= moveForwardBack * throttle * moveFactor;
        propellerForceFL += moveLeftRight * throttle;


        //BR
        float propellerForceBR = throttle + (-PID_pitch_output + PID_roll_output);
        propellerForceBR += moveForwardBack * throttle * moveFactor;
        propellerForceBR -= moveLeftRight * throttle;


        //BL 
        float propellerForceBL = throttle + (-PID_pitch_output - PID_roll_output);
        propellerForceBL += moveForwardBack * throttle * moveFactor;
        propellerForceBL += moveLeftRight * throttle;

        propellerForceFR = Mathf.Clamp(propellerForceFR, 0f, maxPropellerForce);
        propellerForceFL = Mathf.Clamp(propellerForceFL, 0f, maxPropellerForce);
        propellerForceBR = Mathf.Clamp(propellerForceBR, 0f, maxPropellerForce);
        propellerForceBL = Mathf.Clamp(propellerForceBL, 0f, maxPropellerForce);

        AddForceToPropeller(propellerFR, propellerForceFR);
        AddForceToPropeller(propellerFL, propellerForceFL);
        AddForceToPropeller(propellerBR, propellerForceBR);
        AddForceToPropeller(propellerBL, propellerForceBL);

        //Yaw
        //Minimize the yaw error (which is already signed):
        float yawError = quadcopterRB.angularVelocity.y;

        float PID_yaw_output = PID_yaw.GetFactorFromPIDController(PID_yaw_gains, yawError, Time.fixedDeltaTime);

        //First we need to add a force (if any)
        quadcopterRB.AddRelativeTorque(transform.up * yawDir * maxTorque * throttle);

        //Then we need to minimize the error
        quadcopterRB.AddRelativeTorque(transform.up * throttle * PID_yaw_output * -1f);
    }

    void AddForceToPropeller(GameObject propellerObj, float propellerForce)
    {
        Vector3 propellerUp = propellerObj.transform.up;

        Vector3 propellerPos = propellerObj.transform.position;

        quadcopterRB.AddForceAtPosition(propellerUp * propellerForce, propellerPos);

        //Debug
        //Debug.DrawRay(propellerPos, propellerUp * 1f, Color.red);
    }

    private float GetPitchError()
    {
        xAngle = transform.eulerAngles.x;

        xAngle = WrapAngle(xAngle);

        if (xAngle > 180f && xAngle < 360f)
        {
            xAngle = 360f - xAngle;

            //-1 so we know if we are pitching back or forward
            xAngle *= -1f;
        }
        
        return xAngle;
    }

    private float GetRollError()
    {
        zAngle = transform.eulerAngles.z;

        zAngle = WrapAngle(zAngle);

        if (zAngle > 180f && zAngle < 360f)
        {
            zAngle = 360f - zAngle;

            //-1 so we know if we are rolling left or right
            zAngle *= -1f;
        }
        return zAngle;
    }

    //Wrap between 0 and 360 degrees
    float WrapAngle(float inputAngle)
    {
        //The inner % 360 restricts everything to +/- 360
        //+360 moves negative values to the positive range, and positive ones to > 360
        //the final % 360 caps everything to 0...360
        return ((inputAngle % 360f) + 360f) % 360f;
    }

    private float GetAltitudeError(){
        return targetAltitude - transform.position.y;
    }


    public bool goToPosition(Vector3 goal){
        float tol = 0.05f;

        
        
        if(goal.y > 0.2){
            targetAltitude = goal.y;
        }

        if(Vector3.Distance(goal,transform.position) < tol && Vector3.Distance(goal,transform.position) > -tol ){
            Debug.Log("Goal reach !!!");
            return true;
        }
        return false;
    }

    //Add external forces to the quadcopter, such as wind
    // private void AddExternalForces()
    // {
    //     Vector3 windDir = -Vector3.forward;

    //     windDir = Quaternion.Euler(0, forceDir, 0) * windDir;

    //     quadcopterRB.AddForce(windDir * windForce);
    // }
}