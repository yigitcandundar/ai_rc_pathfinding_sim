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

    private float distanceToPathObjective = 0.0f;

    private float steerAngleThreshold = 9;
    private float forwardDistanceThreshold = 0.5f;

    private float minForwardTorqueMultiplier = 0.5f;
    private float minSteerTorqueMultiplier = 0.5f;

    [SerializeField]
    private float forwardTorqueMultiplier = 0.0f;

    [SerializeField]
    private float steerTorqueMultiplier = 0.0f;

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
            timerText.text = (maxTime - (Time.time - timer)).ToString("F2");
            
            CheckForFailures();

            switch (currentState)
            {
                case CarState.Search:
                    stateText.text = "Searching";



                    break;
                case CarState.Goal:
                    stateText.text = "Moving to goal";

                    pathPos = proxSensor.GetOptimalPathToPosition(transform.position, targetPos);

                    followBall.position = pathPos;

                    distToGoal = Vector3.Distance(transform.position, targetPos);
                    angleToGoal = Vector3.Angle(pathPos - transform.position, transform.forward);

                    distanceToPathObjective = Vector3.Distance(transform.position, pathPos);

                    forwardTorqueMultiplier = Mathf.Clamp((distanceToPathObjective - forwardDistanceThreshold) / forwardDistanceThreshold, minForwardTorqueMultiplier, 1);

                    if (angleToGoal > angleTreshold)
                    {
                        steerPos = transform.InverseTransformPoint(pathPos);
                        steerTorqueMultiplier = Mathf.Clamp((angleToGoal - steerAngleThreshold) / steerAngleThreshold, minSteerTorqueMultiplier, 1);
                        //Right
                        if (steerPos.x > 0.2f)
                        {
                            carMotor.TurnRight(1);
                        }
                        //Left
                        else if (steerPos.x < -0.2f)
                        {
                            carMotor.TurnLeft(1);
                        }
                        else
                        {
                            carMotor.GoForward(forwardTorqueMultiplier);
                        }
                    }
                    else
                    {
                        //Move Forward
                        carMotor.GoForward(forwardTorqueMultiplier);
                    }

                    break;
                case CarState.Fail:
                    stateText.text = "Failed";
                    break;
            }
        }
	}

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

    public void FoundGoal(GameObject goalObject)
    {
        if (currentState != CarState.Goal)
        {
            targetPos = goalObject.transform.position;
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
                    targetPos = goalObject.transform.position;
                    currentState = CarState.Goal;
                }

                lastTargetSwitch = Time.time;
            }
        }
    }

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
}
