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
    
    private Vector3 wheel_pos = new Vector3();
    private Quaternion left_rotation = new Quaternion();
    private Quaternion right_rotation = new Quaternion();

    [SerializeField]
    private float currentVelocity = 0f;
    [SerializeField]
    private float maxVelocity = 3f;

    [SerializeField]
    private float leftTorque = 0f;
    [SerializeField]
    private float rightTorque = 0f;


    [SerializeField]
    private float acceleration = 10f;
    [SerializeField]
    private float deceleration = 20f;
    
    private bool forwardInput = false;
    private bool leftInput = false;
    private bool rightInput = false;
    private bool backInput = false;
    

    private void Update()
    {
        forwardImage.color = Color.white;
        backImage.color = Color.white;
        leftImage.color = Color.white;
        rightImage.color = Color.white;

        currentVelocity = mainBody.velocity.magnitude;

        if (currentVelocity < maxVelocity)
        {
            if (forwardInput)
            {
                forwardImage.color = Color.green;

                leftTorque = torqueLimit;
                rightTorque = torqueLimit;

                //leftTorque = Mathf.Clamp(leftTorque + acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
                //rightTorque = Mathf.Clamp(rightTorque + acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
            }
            else if (leftInput)
            {
                leftImage.color = Color.green;

                leftTorque = -torqueLimit;
                rightTorque = torqueLimit;

                //leftTorque = Mathf.Clamp(leftTorque - acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
                //rightTorque = Mathf.Clamp(rightTorque + acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
            }
            else if (rightInput)
            {
                rightImage.color = Color.green;

                leftTorque = torqueLimit;
                rightTorque = -torqueLimit;

                //leftTorque = Mathf.Clamp(leftTorque + acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
                //rightTorque = Mathf.Clamp(rightTorque - acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
            }
            else if (backInput)
            {
                backImage.color = Color.green;

                leftTorque = -torqueLimit;
                rightTorque = -torqueLimit;

                //leftTorque = Mathf.Clamp(leftTorque - acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
                //rightTorque = Mathf.Clamp(rightTorque - acceleration * Time.deltaTime, -torqueLimit, torqueLimit);
            }
            else
            {
                leftTorque = 0;
                rightTorque = 0;

                //if (leftTorque < 0)
                //{
                //    leftTorque = Mathf.Clamp(leftTorque + deceleration * Time.deltaTime, -torqueLimit, 0);
                //}
                //else
                //{
                //    leftTorque = Mathf.Clamp(leftTorque - deceleration * Time.deltaTime, 0, torqueLimit);
                //}
                //if (rightTorque < 0)
                //{
                //    rightTorque = Mathf.Clamp(rightTorque + deceleration * Time.deltaTime, -torqueLimit, 0);
                //}
                //else
                //{
                //    rightTorque = Mathf.Clamp(rightTorque - deceleration * Time.deltaTime, 0, torqueLimit);
                //}
            }
        }
        else
        {
            leftTorque = 0;
            rightTorque = 0;

            //if (leftTorque < 0)
            //{
            //    leftTorque = Mathf.Clamp(leftTorque + deceleration * Time.deltaTime, -torqueLimit, 0);
            //}
            //else
            //{
            //    leftTorque = Mathf.Clamp(leftTorque - deceleration * Time.deltaTime, 0, torqueLimit);
            //}
            //if(rightTorque < 0)
            //{
            //    rightTorque = Mathf.Clamp(rightTorque + deceleration * Time.deltaTime, -torqueLimit, 0);
            //}
            //else
            //{
            //    rightTorque = Mathf.Clamp(rightTorque - deceleration * Time.deltaTime, 0, torqueLimit);
            //}
        }

        wheel_left.motorTorque = leftTorque;
        wheel_right.motorTorque = rightTorque;

        // Update wheel rotations
        wheel_left.GetWorldPose(out wheel_pos, out left_rotation);
        wheel_right.GetWorldPose(out wheel_pos, out right_rotation);

        wheel_model_left.rotation = left_rotation;
        wheel_model_right.rotation = right_rotation;

        forwardInput = false;
        leftInput = false;
        rightInput = false;
        backInput = false;

    }

    public void TurnLeft(float multiplier)
    {
        leftInput = true;
    }
    public void TurnRight(float multiplier)
    {
        rightInput = true;
    }
    public void GoForward(float multiplier)
    {
        forwardInput = true;
    }
    public void GoBackward()
    {
        backInput = true;
    }
}
