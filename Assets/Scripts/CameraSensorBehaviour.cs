using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSensorBehaviour : MonoBehaviour {

    public CarAI parentAI;
    public Camera sensorCam;
    private GoalManager gManager;
    private bool observeSurroundings = false;

    private void Start()
    {
        gManager = GameObject.Find("Goals").GetComponent<GoalManager>();

        if (gManager != null)
        {
            observeSurroundings = true;
        }
    }

    private void FixedUpdate()
    {
        if (observeSurroundings)
        {
            foreach(GameObject goal in gManager.goals)
            {
                if (IsInView(gameObject, goal))
                {
                    parentAI.FoundGoal(goal);
                }
            }
        }
    }

    //Check if a goal object is in view of the camera
    private bool IsInView(GameObject origin, GameObject toCheck)
    {
        Vector3 pointOnScreen = sensorCam.WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            return false;
        }

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            return false;
        }

        RaycastHit hit;
        Vector3 heading = toCheck.transform.position - origin.transform.position;
        Vector3 direction = heading.normalized;

        if (Physics.Linecast(sensorCam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        {
            if (hit.transform.name != toCheck.name)
            {
                return false;
            }
        }
        return true;
    }
}
