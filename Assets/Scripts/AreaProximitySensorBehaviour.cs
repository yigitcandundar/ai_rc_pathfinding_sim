using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaProximitySensorBehaviour : MonoBehaviour {

    private float maxDistance = 4f;

    private List<Collider> collisions = new List<Collider>();
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Goal" && other.tag != "Player")
        {
            collisions.Add(other);
        }
    }
    public Vector3 GetOptimalPathToPosition(Vector3 fromPosition,Vector3 targetPosition)
    {
        Vector3 directionToGoal = (targetPosition - fromPosition).normalized;
        Vector3 directionFromCollision = Vector3.zero;

        List<Ray> raysToEverything = new List<Ray>();

        float distanceSum = 0.0f;
        Vector3 directionSum = Vector3.zero;

        float avgDistanceToCollision = maxDistance;
        Debug.DrawRay(fromPosition, (targetPosition - fromPosition));
        foreach (Collider collider in collisions)
        {
            Vector3 closestPoint = collider.ClosestPoint(fromPosition);
            directionFromCollision = -(closestPoint - fromPosition).normalized;

            float distanceToCollision = Vector3.Distance(fromPosition, closestPoint);

            distanceSum += distanceToCollision;
            
            if (distanceToCollision <= maxDistance)
            {
                directionFromCollision *= Mathf.Clamp(maxDistance - Mathf.Clamp(distanceToCollision, 0, maxDistance), 0.5f, maxDistance);
                directionSum += directionFromCollision;

                Debug.DrawRay(fromPosition, (closestPoint - fromPosition), Color.red);
            }
            else
            {
                Debug.DrawRay(fromPosition, (closestPoint - fromPosition),Color.green);
            }
        }

        if (collisions.Count > 0)
        {
            avgDistanceToCollision = distanceSum / collisions.Count;
        }
        directionToGoal *= Mathf.Clamp(avgDistanceToCollision, 1, maxDistance);

        return fromPosition + (directionToGoal) + (directionSum);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Goal" && other.tag != "Player")
        {
            collisions.Remove(other);
        }
    }
}
