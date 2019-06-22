using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour {

    public List<GameObject> goals = new List<GameObject>();
    public Transform carTransform;
    private float totalDistanceBtwGoals = 0.0f;

    private void Start()
    {
        foreach(Transform obj in transform)
        {
            if (obj.tag == "Goal" && obj.gameObject.activeInHierarchy)
            {
                AddGoal(obj.gameObject);
            }
        }

        if (goals.Count > 0)
        {
            totalDistanceBtwGoals += Vector3.Distance(carTransform.position, goals[0].transform.position);
        }

        for (int i = 0; i < goals.Count; i++)
        {
            if(i + 1 < goals.Count)
            {
                totalDistanceBtwGoals += Vector3.Distance(goals[i].transform.position, goals[i + 1].transform.position);
            }
        }

        Debug.Log("Total Approximate Distance: " + totalDistanceBtwGoals.ToString());
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
