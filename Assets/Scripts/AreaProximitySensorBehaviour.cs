using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaProximitySensorBehaviour : MonoBehaviour {

    // Minimum distance allowed from a proximity object
    private float minDistance = 4f;
    private float proxSensorRadius = 7f;

    // List of detected objects inside the proximity
    private List<Collider> collisions = new List<Collider>();

    //List of all recorder local minima sections within a simulation session
    private List<Vector3> minimaPositions = new List<Vector3>();

    [SerializeField]
    private Vector3 directionToGoal;
    [SerializeField]
    private Vector3 directionSum;
    [SerializeField]
    private Vector3 minimaDirSum;
    [SerializeField]
    float avgDistanceToCollision = 0;

    // The radius of the frontal collision check cone
    private float frontDetectionConeRadius = 0.7f;

    // Detect objects in proximity
    private void OnTriggerEnter(Collider other)
    {
        //If the detected object is not ourself or the goal object, add it to the list of current collisions
        if (other.tag != "Goal" && other.tag != "Player")
        {
            collisions.Add(other);
        }
    }

    // Called when a proximity object gets out of the proximity area
    private void OnTriggerExit(Collider other)
    {
        //If the detected object is not ourself or the goal object, remove it from the list of current collisions
        if (other.tag != "Goal" && other.tag != "Player")
        {
            collisions.Remove(other);
        }
    }

    // Calculate collision free path using Potential Field inspired logic and return the arbitrary target position
    public Vector3 GetOptimalPathToPosition(Vector3 fromPosition,Vector3 targetPosition)
    {
        // Direction from the current position of the car to its current goal
        directionToGoal = (targetPosition - fromPosition).normalized;

        // Direction away from the current position and current proximity objects
        Vector3 directionFromCollision = Vector3.zero;
        
        // The float sum of distance
        float distanceSum = 0.0f;
        int criticalCollisionsCount = 0;

        // The final sum of directions from proximity objects
        directionSum = Vector3.zero;
        minimaDirSum = Vector3.zero;
        // The average distance to proximity collisions
        avgDistanceToCollision = minDistance;

        //DEBUG CODE: Draw rays to the goal in the editor window
        Debug.DrawRay(fromPosition, (targetPosition - fromPosition));
        
        // Iterate over every proximity object and adjust the away direction that will be used to find the collision-free path
        foreach (Collider collider in collisions)
        {
            // Find the closest point to the proximity object from the cars position
            Vector3 closestPoint = collider.ClosestPoint(fromPosition);
            
            // Calculate distance to the proximity object
            float distanceToCollision = Vector3.Distance(fromPosition, closestPoint);
            
            // If the distance to current proximity object is dangerously close, amplify the direction and contribute to the actual path calculation
            if (distanceToCollision <= minDistance)
            {
                criticalCollisionsCount++;
                // Find the inverse of the direction to the object
                directionFromCollision = -(closestPoint - fromPosition).normalized;

                // Add the new distance to the distance sum
                distanceSum += distanceToCollision;

                //Find the appropriate direction away from the obstacle
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

        //Check if a recorded local minima is close by
        foreach(Vector3 localMinima in minimaPositions)
        {
            float distanceToLocalMinima = Vector3.Distance(fromPosition, localMinima);

            Vector3 dirFromMinimaToPos = localMinima;

            if (distanceToLocalMinima > minDistance)
            {
                dirFromMinimaToPos += minDistance * (fromPosition - localMinima).normalized;
                distanceToLocalMinima = Vector3.Distance(fromPosition, dirFromMinimaToPos);

                if (distanceToLocalMinima < proxSensorRadius)
                {
                    directionFromCollision = -(dirFromMinimaToPos - fromPosition).normalized;
                    directionFromCollision *= Mathf.Clamp(proxSensorRadius - Mathf.Clamp(distanceToLocalMinima, 0, proxSensorRadius), 0.5f, proxSensorRadius);

                    minimaDirSum += directionFromCollision;

                    Debug.DrawRay(fromPosition, (dirFromMinimaToPos - fromPosition), Color.yellow);
                }
            }
            else
            {
                if (distanceToLocalMinima < proxSensorRadius)
                {
                    directionFromCollision = -(dirFromMinimaToPos - fromPosition).normalized * minDistance;

                    directionSum += directionFromCollision;

                    Debug.DrawRay(fromPosition, (dirFromMinimaToPos - fromPosition), Color.yellow);
                }
            }
        }

        // Check for division by 0 cases
        if (criticalCollisionsCount > 0)
        {
            // Calculate average distance to collisions
            avgDistanceToCollision = (distanceSum / criticalCollisionsCount) / minDistance;
        }

        // Calculate the direction to the goal with average distance to collision to adjust its magnitude
        directionSum *= Mathf.Clamp(avgDistanceToCollision, 1, minDistance);

        // Return the collision free path using direction to goal and the direction away from proximity objects
        return fromPosition + (2 * directionToGoal) + directionSum + minimaDirSum;
    }

    //Returns true if there is an object in front of the car (Works for directly in front and frontal cone area proximity)
    public bool HasObjectInFront(Transform fromTransform)
    {
        bool result = false;
        RaycastHit hit;
        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        //Check if there is something directly in front of the car
        if (Physics.Raycast(new Ray(fromTransform.position, fromTransform.forward), out hit, minDistance,layerMask,QueryTriggerInteraction.UseGlobal))
        {
            if (hit.collider.tag != "Player" && hit.collider.tag != "Goal")
            {
                Debug.DrawRay(fromTransform.position, fromTransform.forward * minDistance, Color.red);
                result = true;
            }
        }
        else //Check if there is something to the sides of the car
        {
            Vector3 forwardLeft = new Vector3(fromTransform.forward.x - frontDetectionConeRadius, fromTransform.forward.y, fromTransform.forward.z);
            Vector3 forwardRight = new Vector3(fromTransform.forward.x + frontDetectionConeRadius, fromTransform.forward.y, fromTransform.forward.z);

            //Check the leftmost side of the frontal cone
            if (Physics.Raycast(new Ray(fromTransform.position, forwardLeft), out hit, minDistance, layerMask, QueryTriggerInteraction.UseGlobal))
            {
                if (hit.collider.tag != "Player" && hit.collider.tag != "Goal")
                {
                    Debug.DrawRay(fromTransform.position, forwardLeft * minDistance, Color.red);
                    result = true;
                }
            }
            //Check the rightmost side of the frontal cone
            if (Physics.Raycast(new Ray(fromTransform.position, forwardRight), out hit, minDistance, layerMask, QueryTriggerInteraction.UseGlobal))
            {
                if (hit.collider.tag != "Player" && hit.collider.tag != "Goal")
                {
                    Debug.DrawRay(fromTransform.position, forwardRight * minDistance, Color.red);
                    result = true;
                }
            }

            //Try and see if the proximity object is inside the frontal cone range
            foreach (Collider collider in collisions)
            {
                Vector3 closestPoint = collider.ClosestPoint(fromTransform.position);
                Vector3 dir = (closestPoint - fromTransform.position).normalized;
                Vector3 relativePosition = fromTransform.InverseTransformPoint(fromTransform.position + dir);

                if (relativePosition.z > 0)
                {
                    if (relativePosition.x < frontDetectionConeRadius && relativePosition.x > -frontDetectionConeRadius)
                    {
                        Debug.DrawRay(fromTransform.position, (closestPoint - fromTransform.position), Color.red);
                        result = true;
                    }
                }
            }
        }

        return result;
    }

    //Returns true if there are either 3 or more obstacles in proximity or if there are less than 3 obstacles, checks if any obstacle is in critical distance then returns true again
    public bool HasObjectInCloseProximity(Vector3 fromPosition)
    {
        if (collisions.Count >= 3)
        {
            return true;
        }
        else
        {
            foreach (Collider collider in collisions)
            {
                // Find the closest point to the proximity object from the cars position
                Vector3 closestPoint = collider.ClosestPoint(fromPosition);

                // Calculate distance to the proximity object
                float distanceToCollision = Vector3.Distance(fromPosition, closestPoint);

                // If the distance to current proximity object is dangerously close, return true
                if (distanceToCollision <= minDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //Called when a local minima is detected to record the local minima and include it in the path finding process
    public void RecordLocalMinimaAtPosition(Vector3 localMinimaPosition)
    {
        bool isDuplicate = false;

        //Check if the minima is a duplicate
        foreach(Vector3 localMinima in minimaPositions)
        {
            if (Vector3.Distance(localMinima, localMinimaPosition) < minDistance)
            {
                isDuplicate = true;
            }
        }

        //If this local minima is a new one, then add it to the list
        if (!isDuplicate)
        {
            minimaPositions.Add(localMinimaPosition);
        }
    }
}