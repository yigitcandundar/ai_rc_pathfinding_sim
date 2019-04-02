using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorBehaviour : MonoBehaviour
{

    public float torque = 1000f;
    public WheelCollider wheel_left;
    public WheelCollider wheel_right;

    public Transform wheel_model_left;
    public Transform wheel_model_right;

    private Vector3 wheel_pos = new Vector3();
    private Quaternion left_rotation = new Quaternion();
    private Quaternion right_rotation = new Quaternion();

    private bool receivedInput = false;

    void Update()
    {
        receivedInput = false;

        if (Input.GetKey(KeyCode.W))
        {
            receivedInput = true;
            GoForward();
        }
        if (Input.GetKey(KeyCode.A))
        {
            receivedInput = true;
            TurnLeft();
        }
        if (Input.GetKey(KeyCode.D))
        {
            receivedInput = true;
            TurnRight();
        }
        if (Input.GetKey(KeyCode.S))
        {
            receivedInput = true;
            GoBackward();
        }

        if (!receivedInput)
        {
            Brake();
        }

        // Update wheel rotations
        wheel_left.GetWorldPose(out wheel_pos, out left_rotation);
        wheel_right.GetWorldPose(out wheel_pos, out right_rotation);

        wheel_model_left.rotation = left_rotation;
        wheel_model_right.rotation = right_rotation;
    }

    private void TurnLeft()
    {
        wheel_left.motorTorque = -torque;
        wheel_right.motorTorque = torque;
    }
    private void TurnRight()
    {
        wheel_left.motorTorque = torque;
        wheel_right.motorTorque = -torque;
    }
    private void GoForward()
    {
        wheel_left.motorTorque = torque;
        wheel_right.motorTorque = torque;
    }
    private void GoBackward()
    {
        wheel_left.motorTorque = -torque;
        wheel_right.motorTorque = -torque;
    }
    private void Brake()
    {
        wheel_left.motorTorque = 0.0f;
        wheel_right.motorTorque = 0.0f;
    }
}
