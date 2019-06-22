using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class CarAI : MonoBehaviour {

    enum CarState
    {
        Idle,
        Goal
    }
    public Transform followBall;
    public Text stateText;
    public Text pointsText;
    public Text collisionsText;
    public Text timerText;
    public Text timerTitleText;

    public MotorBehaviour carMotor;
    public GoalManager gManager;

    public AreaProximitySensorBehaviour proxSensor;

    public bool recordLocalMinima = true;

    private Vector3 initPos;
    private float mapSize = 500;

    private Rigidbody rBody;
    
    private CarState currentState = CarState.Idle;

    private int points = 0;
    private int collisions = 0;
    private int localMinimas = 0;

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
    private bool stopTimer = false;

    private bool collectionComplete = false;
    private bool ranOutOfTime = false;
    private bool printedResults = false;

    //SET THIS TO TRUE WHEN THE TIMER NEEDS TO COUNT DOWN (CAR CAN FAIL IF IT RUNS OUT OF TIME)
    //SET IT TO FALSE TO RECORD THE TOTAL TIME TAKEN FOR THE CAR TO FINISH THE RUN (CAR CANNOT FAIL THE RUN)
    private bool restrictTime = false;

    private float currentTime = 0.0f;

    private Vector3 pastPosition = Vector3.zero;
    private float totalDistanceTravelled = 0.0f;

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
        timer = Time.time;
	}

    void Update()
    {
        if (currentGoal == null)
        {
            GameObject tmpObj = gManager.GetGoalAtFirstIndex();

            if (tmpObj != null)
            {
                currentGoal = tmpObj.transform;
                targetPos = currentGoal.position;
                currentState = CarState.Goal;
            }
            else
            {
                stopTimer = true;
                collectionComplete = true;
                ranOutOfTime = false;
                currentState = CarState.Idle;
            }
        }

        if (!stopTimer)
        {
            if (restrictTime) //If true the time limit will be forced and the AI might fail the test if it runs out of time
            {
                timerTitleText.text = "Remaining Time:";
                if (Time.time - timer >= maxTime)
                {
                    timerText.text = "0";
                    collectionComplete = false;
                    ranOutOfTime = true;
                    currentState = CarState.Idle;
                }
                else
                {
                    timerText.text = (maxTime - (Time.time - timer)).ToString("F2");
                }
            }
            else // Else the time it takes to complete the run is recorded
            {
                timerTitleText.text = "Time Taken:";
                currentTime += Time.deltaTime;
                timerText.text = currentTime.ToString("F2");
            }
        }
        
        switch (currentState)
        {
            case CarState.Goal:
                stateText.text = "Moving to goal";

                // Get collision free path from the cars proximity sensor
                pathPos = proxSensor.GetOptimalPathToPosition(transform.position, targetPos);

                // DEBUG CODE: Set the visualizer for the current arbitrary path
                followBall.position = pathPos;

                distToGoal = Vector3.Distance(transform.position, targetPos);
                angleToGoal = Vector3.Angle(pathPos - transform.position, transform.forward);

                if (pastPosition != Vector3.zero)
                {
                    totalDistanceTravelled += Vector3.Distance(transform.position, pastPosition);
                    pastPosition = transform.position;
                }
                else
                {
                    pastPosition = transform.position;
                }

                if (wiggleCount < wiggleTarget)
                {
                    if (carMotor.IsAboveBrakeVelocity() && proxSensor.HasObjectInFront(transform))
                    {
                        lastInput = LastInputType.brake;
                        carMotor.Brake();
                    }
                    else
                    {
                        // If the AI is not stuck in a local minima, then proceed with regular movement logic

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
                        }
                        else
                        {
                            // Go Forward
                            CheckForWiggles(LastInputType.forward);
                            lastInput = LastInputType.forward;
                            carMotor.GoForward();
                        }
                    }
                }
                else //If the AI is stuck in a local minima, record the local minima location
                {
                    localMinimas++;

                    if (recordLocalMinima)
                    {
                        proxSensor.RecordLocalMinimaAtPosition(transform.position);
                    }
                    wiggleCount = 0;
                }
                break;
            case CarState.Idle:
                if (collectionComplete)
                {
                    if (!printedResults)
                    {
                        if (restrictTime)
                        {
                            Debug.Log("Points: " + points + " - Collisions: " + collisions + " - Local Minima: " + localMinimas);
                        }
                        else
                        {
                            Debug.Log("Time taken(s): " + currentTime + " - Total Distance Travelled(u): " + totalDistanceTravelled + " - Points: " + points + " - Collisions: " + collisions + " - Local Minima: " + localMinimas);
                        }

                        printedResults = true;
                    }

                    stateText.text = "Collected all known objects!";
                }
                else if (ranOutOfTime)
                {
                    stateText.text = "Ran out of time!";
                }
                else
                {
                    stateText.text = "AI is in Idle mode";
                }
                break;
        }
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
        if (currentState != CarState.Goal) //If the car is idling but manages to spot a missed goal object, set it as current goal and move towards it (EDGE CASE)
        {
            currentGoal = goalObject.transform;
            targetPos = currentGoal.position;
            currentState = CarState.Goal;

            lastTargetSwitch = Time.time;
        }
        else if (currentState == CarState.Goal) //If we already have a goal but we detect another goal object
        {
            if (Time.time - lastTargetSwitch > targetSwitchTimer)
            {
                //If the newfound object is closer than the previously selected obj, set new one as current goal
                float newDistance = Vector3.Distance(transform.position, goalObject.transform.position);
                if (newDistance < distToGoal)
                {
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
        }
    }
    //Detect if the AI collides with an obstacle
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Goal" && collision.gameObject.name != "Terrain")
        {
            Debug.Log("COLLIDED WITH " + collision.gameObject.name);
            collisions++;
            collisionsText.text = collisions.ToString();
        }
    }
}
