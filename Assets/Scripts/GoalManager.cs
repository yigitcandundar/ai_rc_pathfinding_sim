using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour {

    public List<GameObject> goals = new List<GameObject>();

    private void Start()
    {
        foreach(Transform obj in transform)
        {
            if (obj.tag == "Goal")
            {
                AddGoal(obj.gameObject);
            }
        }
    }
    // Get the goal object that is at the top of the list
    public GameObject GetGoalAtFirstIndex()
    {
        GameObject result = null;

        if (goals.Count > 0)
        {
            result = goals[0];
        }
        return result;
    }
    // Add a new goal object to the list
    private void AddGoal(GameObject goal)
    {
        if (!goals.Contains(goal))
        {
            goals.Add(goal);
        }
    }
    // Remove a goal object from the list
    public void RemoveGoal(GameObject goal)
    {
        if (goals.Contains(goal))
        {
            goals.Remove(goal);
        }

        Destroy(goal);
    }
}
