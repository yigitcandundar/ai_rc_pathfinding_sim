using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotorBehaviour : MonoBehaviour
{
    public float torqueLimit = 700f;

    public Rigidbody mainBody;

    public WheelCollider wheel_left;
    public WheelCollider wheel_right;

    public Transform wheel_model_left;
    public Transform wheel_model_right;

    public Image forwardImage;
    public Image backImage;
    public Image leftImage;
    public Image rightImage;
    public Image brakeImage;

    private Vector3 wheel_pos = new Vector3();
    private Quaternion left_rotation = new Quaternion();
    private Quaternion right_rotation = new Quaternion();

    [SerializeField]
    private float currentVelocity = 0f;
    [SerializeField]
    private float maxVelocity = 3f;
    private float brakeVelocity = .5f;

    [SerializeField]
    private float leftTorque = 0f;
    [SerializeField]
    private float rightTorque = 0f;

    
    private bool forwardInput = false;
    private bool leftInput = false;
    private bool rightInput = false;
    private bool backInput = false;
    private bool brakeInput = false;

    private bool manualControl = false;
    // Apply the movement inputs to the wheels of the car
    private void Update()
    {
        brakeImage.color = Color.white;
        forwardImage.color = Color.white;
        backImage.color = Color.white;
        leftImage.color = Color.white;
        rightImage.color = Color.white;

        currentVelocity = mainBody.velocity.magnitude;

        if (manualControl)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                brakeImage.color = Color.green;

                leftTorque = 0;
                rightTorque = 0;
            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                {
                    forwardImage.color = Color.green;

                    leftTorque = torqueLimit;
                    rightTorque = torqueLimit;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    leftImage.color = Color.green;

                    leftTorque = -torqueLimit;
                    rightTorque = torqueLimit;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    rightImage.color = Color.green;

                    leftTorque = torqueLimit;
                    rightTorque = -torqueLimit;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    backImage.color = Color.green;

                    leftTorque = -torqueLimit;
                    rightTorque = -torqueLimit;
                }
                else
                {
                    leftTorque = 0;
                    rightTorque = 0;
                }
            }
        }
        else
        {
            if (currentVelocity < maxVelocity)
            {
                if (brakeInput)
                {
                    brakeImage.color = Color.green;

                    leftTorque = 0;
                    rightTorque = 0;
                }
                else
                {
                    if (forwardInput)
                    {
                        forwardImage.color = Color.green;

                        leftTorque = torqueLimit;
                        rightTorque = torqueLimit;
                    }
                    else if (leftInput)
                    {
                        leftImage.color = Color.green;

                        leftTorque = -torqueLimit;
                        rightTorque = torqueLimit;
                    }
                    else if (rightInput)
                    {
                        rightImage.color = Color.green;

                        leftTorque = torqueLimit;
                        rightTorque = -torqueLimit;
                    }
                    else if (backInput)
                    {
                        backImage.color = Color.green;

                        leftTorque = -torqueLimit;
                        rightTorque = -torqueLimit;
                    }
                    else
                    {
                        leftTorque = 0;
                        rightTorque = 0;
                    }
                }
            }
            else
            {
                leftTorque = 0;
                rightTorque = 0;
            }
        }
        wheel_left.motorTorque = leftTorque;
        wheel_right.motorTorque = rightTorque;

        // Update wheel rotations (VISUAL)
        wheel_left.GetWorldPose(out wheel_pos, out left_rotation);
        wheel_right.GetWorldPose(out wheel_pos, out right_rotation);

        wheel_model_left.rotation = left_rotation;
        wheel_model_right.rotation = right_rotation;

        brakeInput = false;
        forwardInput = false;
        leftInput = false;
        rightInput = false;
        backInput = false;
    }
    public bool IsAboveBrakeVelocity()
    {
        return currentVelocity > brakeVelocity;
    }
    public void TurnLeft()
    {
        leftInput = true;
    }
    public void TurnRight()
    {
        rightInput = true;
    }
    public void GoForward()
    {
        forwardInput = true;
    }
    public void GoBackward()
    {
        backInput = true;
    }
    public void Brake()
    {
        brakeInput = true;
    }
}
