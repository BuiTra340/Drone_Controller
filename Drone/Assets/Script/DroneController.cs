using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DroneController : MonoBehaviour
{
    [SerializeField] private Joystick joystickMovement;
    [SerializeField] private Joystick joystickControlHeightAndRotation;
    [SerializeField]private float moveSpeed;
    [SerializeField] private float moveSpeedDefault;
    private float speedPerFrame;
    private float tartgetSpeed;
    [SerializeField] private float heightAndRotationSpeed;
    private Rigidbody rigidbody;
    private bool isTilting;
    private const float timeToModifySpeed = 3f;
    private const int tiltForce = 5;

    [SerializeField] private TextMeshProUGUI heightText;
    [SerializeField] private TextMeshProUGUI speedText;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ConstraintsYPosition();
        TiltDrone();
        HeightAndSpeedUI();
        LimitSpeedWithTime();
    }

    private void FixedUpdate()
    {
        MoveDrone(joystickMovement.inputVector);
        ControlHeightAndRotation(joystickControlHeightAndRotation.inputVector);
    }

    private void LimitSpeedWithTime()
    {
        if(joystickMovement.inputVector.magnitude == 0)
        {
            moveSpeed = 0;
        }

        if (joystickMovement.isJoystickMovement && joystickMovement.isJoystickPositionNotChange())
        {
            if (moveSpeed < tartgetSpeed)
            {
                moveSpeed += speedPerFrame * Time.deltaTime;
            }
            else if (moveSpeed > tartgetSpeed)
            {
                moveSpeed -= speedPerFrame * Time.deltaTime;
            }
        }
        else if(!joystickMovement.isJoystickPositionNotChange())
        {
            float previousSpeed = moveSpeed;
            tartgetSpeed = moveSpeedDefault * joystickMovement.inputVector.magnitude;
            speedPerFrame = Mathf.Abs((tartgetSpeed - previousSpeed)) / timeToModifySpeed;
        }
    }

    private void ConstraintsYPosition()
    {
        if (joystickControlHeightAndRotation.inputVector.y != 0)
        {
            rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
        }
    }

    private void HeightAndSpeedUI()
    {
        heightText.text = transform.position.y.ToString("0m");
        if (joystickMovement.inputVector.magnitude !=0)
        {
            speedText.text = ((int)Mathf.Abs(rigidbody.velocity.magnitude)).ToString() + "km/h";
        }else
        {
            speedText.text = "0km/h";
        }
    }

    private void TiltDrone()
    {
        int xRotation = AxisXRotation(joystickMovement.inputVector.y);
        int zRotation = AxisZRotation(joystickMovement.inputVector.x);
        if (!isTilting)
        {
            StartCoroutine(TiltDroneWhenMove(xRotation, zRotation));
        }
    }
    private void MoveDrone(Vector2 inputVector)
    {
        Vector3 movementDirection = transform.forward * inputVector.y + transform.right * inputVector.x;
        rigidbody.velocity = movementDirection * moveSpeed;
    }

    private void ControlHeightAndRotation(Vector2 inputVector)
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, inputVector.y, rigidbody.velocity.z) * heightAndRotationSpeed;
        transform.Rotate(0, inputVector.x, 0);
    }

    private int AxisXRotation(float xRotation)
    {
        if (xRotation > 0.1f)
        {
            return tiltForce;
        }
        else if (xRotation < -0.1f)
        {
            return -tiltForce;
        }
        return 0;
    }

    private int AxisZRotation(float zRotation)
    {
        if (zRotation > 0.1f)
        {
            return -tiltForce;
        }
        else if (zRotation < -0.1f)
        {
            return tiltForce;
        }
        return 0;
    }

    private IEnumerator TiltDroneWhenMove(float xRotation, float zRotation)
    {
        isTilting = true;
        float duration = 0;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, zRotation);
        if (startRotation == targetRotation)
        {
            isTilting = false;
            yield break;
        }
        while (duration < 0.2f)
        {
            duration += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, duration / 0.2f);
            yield return null;
        }
        transform.rotation = targetRotation;
        isTilting = false;
    }
}
