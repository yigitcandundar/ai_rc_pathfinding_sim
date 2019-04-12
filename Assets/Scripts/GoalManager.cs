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
    private void AddGoal(GameObject goal)
    {
        if (!goals.Contains(goal))
        {
            goals.Add(goal);
        }
    }
    public void RemoveGoal(GameObject goal)
    {
        if (goals.Contains(goal))
        {
            goals.Remove(goal);
        }

        Destroy(goal);
    }
}
