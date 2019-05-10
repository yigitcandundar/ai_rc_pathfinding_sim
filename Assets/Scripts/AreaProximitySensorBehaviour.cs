using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaProximitySensorBehaviour : MonoBehaviour {

    // Minimum distance allowed from a proximity object
    private float minDistance = 4f;

    // List of detected objects inside the proximity
    private List<Collider> collisions = new List<Collider>();
    
    // Detect objects in proximity
    private void OnTriggerEnter(Collider other)
    {
        //If the detected object is not ourself or the goal object, add it to the list of current collisions
        if (other.tag != "Goal" && other.tag != "Player")
        {
            collisions.Add(other);
        }
    }
    // Calculate collision free path using Potential Field inspired logic and return the arbitrary target position
    public Vector3 GetOptimalPathToPosition(Vector3 fromPosition,Vector3 targetPosition)
    {
        // Direction from the current position of the car to its current goal
        Vector3 directionToGoal = (targetPosition - fromPosition).normalized;

        // Direction away from the current position and current proximity objects
        Vector3 directionFromCollision = Vector3.zero;

        // A list of rays to every object in proximity (FOR DEBUGGING PURPOSES)
        List<Ray> raysToEverything = new List<Ray>();

        // The float sum of distance
        float distanceSum = 0.0f;

        // The final sum of directions from proximity objects
        Vector3 directionSum = Vector3.zero;

        // The average distance to proximity collisions
        float avgDistanceToCollision = minDistance;

        //DEBUG CODE: Draw rays to the goal in the editor window
        Debug.DrawRay(fromPosition, (targetPosition - fromPosition));
        
        // Iterate over every proximity object and adjust the away direction that will be used to find the collision-free path
        foreach (Collider collider in collisions)
        {
            // Find the closest point to the proximity object from the cars position
            Vector3 closestPoint = collider.ClosestPoint(fromPosition);

            // Find the inverse of the direction to the object
            directionFromCollision = -(closestPoint - fromPosition).normalized;

            // Calculate distance to the proximity object
            float distanceToCollision = Vector3.Distance(fromPosition, closestPoint);

            // Add the new distance to the distance sum
            distanceSum += distanceToCollision;
            
            // If the distance to current proximity object is dangerously close, amplify the direction and contribute to the actual path calculation
            if (distanceToCollision <= minDistance)
            {
                directionFromCollision *= Mathf.Clamp(minDistance - Mathf.Clamp(distanceToCollision, 0, minDistance), 0.5f, minDistance);
                directionSum += directionFromCollision;

                Debug.DrawRay(fromPosition, (closestPoint - fromPosition), Color.red);
            }
            else
            {
                // If the distance to object is safe, just draw a green line to it on the editor window (DEBUG)
                Debug.DrawRay(fromPosition, (closestPoint - fromPosition),Color.green);
            }
        }

        // Check for division by 0 cases
        if (collisions.Count > 0)
        {
            // Calculate average distance to collisions
            avgDistanceToCollision = distanceSum / collisions.Count;
        }

        // Calculate the direction to the goal with average distance to collision to adjust its magnitude
        directionToGoal *= Mathf.Clamp(avgDistanceToCollision, 1, minDistance);

        // Return the collision free path using direction to goal and the direction away from proximity objects
        return fromPosition + (directionToGoal) + (directionSum);
    }

    // Called when a proximity object gets out of the proximity area
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Goal" && other.tag != "Player")
        {
            collisions.Remove(other);
        }
    }
}
