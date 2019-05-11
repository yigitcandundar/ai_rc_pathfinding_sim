using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class CarAI : MonoBehaviour {

    enum CarState
    {
        Search,
        Goal,
        Fail
    }
    public Transform followBall;
    public Text stateText;
    public Text pointsText;
    public Text timerText;

    public MotorBehaviour carMotor;
    public GoalManager gManager;

    public AreaProximitySensorBehaviour proxSensor;

    private Vector3 initPos;
    private float mapSize = 500;

    private Rigidbody rBody;

    private float lastFailCheck = 0;
    private float failCheckInterval = 5; //In seconds

    private CarState currentState = CarState.Search;

    private float points = 0;
    private float timer = 0;
    private float maxTime = 300;

    private Vector3 targetPos = Vector3.zero;
    private Vector3 pathPos = Vector3.zero;
    private Vector3 steerPos = Vector3.zero;

    [SerializeField]
    private float angleToGoal = 0;
    private float angleTreshold = 10;
    [SerializeField]
    private float distToGoal = 0;
    private float lastTargetSwitch = 0;
    private float targetSwitchTimer = 10;
    
    private int currentGoalIndex = 0;
    private Transform currentGoal;
    [SerializeField]
    private int wiggleCount = 0;
    private int wiggleTarget = 10;

    private enum LastInputType
    {
        forward,
        backward,
        right,
        left,
        brake
    }

    private LastInputType lastInput = LastInputType.forward;

    void Start () {
        initPos = transform.position;
        rBody = GetComponent<Rigidbody>();
        lastFailCheck = Time.time;
        timer = Time.time;
	}
	
	void Update () {

        if (Time.time - timer >= maxTime)
        {
            timerText.text = "0";
            stateText.text = "Simulation finished!";
        }
        else
        {
            if (currentGoal == null)
            {
                GameObject tmpObj = gManager.GetGoalAtFirstIndex();

                if (tmpObj != null)
                {
                    FoundGoal(tmpObj);
                }
            }

            timerText.text = (maxTime - (Time.time - timer)).ToString("F2");
            
            CheckForFailures();

            switch (currentState)
            {
                case CarState.Search:
                    stateText.text = "Searching";

                    //Implement basic roaming around functionality

                    break;
                case CarState.Goal:
                    stateText.text = "Moving to goal";

                    // Get collision free path from the cars proximity sensor
                    pathPos = proxSensor.GetOptimalPathToPosition(transform.position, targetPos);

                    // DEBUG CODE: Set the visualizer for the current arbitrary path
                    followBall.position = pathPos;

                    distToGoal = Vector3.Distance(transform.position, targetPos);
                    angleToGoal = Vector3.Angle(pathPos - transform.position, transform.forward);

                    if (carMotor.IsAboveBrakeVelocity() && proxSensor.HasObjectInFront(transform))
                    {
                        Debug.Log("BRAKING");
                        lastInput = LastInputType.brake;
                        carMotor.Brake();
                    }
                    else
                    {
                        if (angleToGoal > angleTreshold)
                        {
                            steerPos = transform.InverseTransformPoint(pathPos);
                            
                            if (steerPos.z > 0)
                            {
                                if (steerPos.x > 0.2f)
                                {
                                    // Steer Right
                                    CheckForWiggles(LastInputType.right);
                                    lastInput = LastInputType.right;

                                    carMotor.TurnRight();
                                }
                                else if (steerPos.x < -0.2f)
                                {
                                    // Steer Left
                                    CheckForWiggles(LastInputType.left);
                                    lastInput = LastInputType.left;
                                    carMotor.TurnLeft();
                                }
                                else
                                {   // Go Forward
                                    CheckForWiggles(LastInputType.forward);
                                    lastInput = LastInputType.forward;
                                    carMotor.GoForward();
                                }
                            }
                            else
                            {
                                // If the object is behind the car, Steer Right or Left accordingly
                                if (wiggleCount < wiggleTarget)
                                {
                                    if (steerPos.x >= 0f)
                                    {
                                        CheckForWiggles(LastInputType.right);
                                        lastInput = LastInputType.right;
                                        carMotor.TurnRight();
                                    }
                                    else
                                    {
                                        CheckForWiggles(LastInputType.left);
                                        lastInput = LastInputType.left;
                                        carMotor.TurnLeft();
                                    }
                                }
                                else
                                {
                                    //Try to escape from local minima
                                    if (lastInput == LastInputType.right)
                                    {
                                        if (!proxSensor.HasObjectInFront(transform))
                                        {
                                            carMotor.GoForward();
                                        }
                                        else
                                        {
                                            carMotor.TurnLeft();
                                        }
                                    }
                                    else
                                    {
                                        if (!proxSensor.HasObjectInFront(transform))
                                        {
                                            carMotor.GoForward();
                                        }
                                        else
                                        {
                                            carMotor.TurnLeft();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Go Forward
                            CheckForWiggles(LastInputType.forward);
                            lastInput = LastInputType.forward;
                            carMotor.GoForward();
                        }
                    }
                    break;
                case CarState.Fail:
                    stateText.text = "Failed";
                    break;
            }
        }
	}

    // Check if the car is upside down
    private void CheckForFailures()
    {
        //if (Time.time - lastFailCheck >= failCheckInterval)
        //{
        //    Ray rayUp = new Ray(transform.position, transform.up);

        //    if (Physics.Raycast(rayUp,1) && rBody.velocity.magnitude < 0.1f)
        //    {
        //        currentState = CarState.Fail;
        //    }

        //    lastFailCheck = Time.time;
        //}
    }

    private void CheckForWiggles(LastInputType currentInput)
    {
        switch (currentInput)
        {
            case LastInputType.forward:
                if (lastInput == LastInputType.backward)
                {
                    wiggleCount++;
                }
                else if (lastInput != LastInputType.forward)
                {
                    wiggleCount = 0;
                }
                break;
            case LastInputType.backward:
                if (lastInput == LastInputType.forward)
                {
                    wiggleCount++;
                }
                else if (lastInput != LastInputType.backward)
                {
                    wiggleCount = 0;
                }
                break;
            case LastInputType.left:
                if (lastInput == LastInputType.right)
                {
                    wiggleCount++;
                }
                else if (lastInput != LastInputType.left)
                {
                    wiggleCount = 0;
                }
                break;
            case LastInputType.right:
                if (lastInput == LastInputType.left)
                {
                    wiggleCount++;
                }
                else if (lastInput != LastInputType.right)
                {
                    wiggleCount = 0;
                }
                break;
        }
    }

    // Called when the camera finds a goal object
    public void FoundGoal(GameObject goalObject)
    {
        if (currentState != CarState.Goal)
        {
            currentGoal = goalObject.transform;
            targetPos = currentGoal.position;
            currentState = CarState.Goal;

            lastTargetSwitch = Time.time;
        }
        else if (currentState == CarState.Goal)
        {
            if (Time.time - lastTargetSwitch > targetSwitchTimer)
            {
                //If the newfound object is closer than the previously selected obj, set new one as current goal
                float newDistance = Vector3.Distance(transform.position, goalObject.transform.position);
                if (newDistance < distToGoal)
                {
                    Debug.Log("Switched targets!");
                    currentGoal = goalObject.transform;
                    targetPos = currentGoal.position;
                    currentState = CarState.Goal;
                }

                lastTargetSwitch = Time.time;
            }
        }
    }

    // Check if we reached the goal object
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal") //If the collided object is a goal object...
        {
            gManager.RemoveGoal(other.gameObject); //Remove goal from scene and goal list

            points++; //Increment points
            pointsText.text = points.ToString(); //Update points display

            currentState = CarState.Search; //Return to search state
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Goal" && collision.gameObject.name != "Terrain")
        {
            Debug.Log("COLLIDED WITH " + collision.gameObject.name);
        }
    }
}
