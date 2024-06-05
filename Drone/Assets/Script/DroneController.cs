using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DroneController : MonoBehaviour
{
    [SerializeField] private Joystick joystickMovement;
    [SerializeField] private Joystick joystickControlHeightAndRotation;
    private float moveSpeed;
    [SerializeField]private float moveSpeedDefault;
    [SerializeField] private float heightAndRotationSpeed;
    private Rigidbody rigidbody;
    private bool isTilting;
    private bool isSpeedUpdating;
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
        speedText.text = ((int)Mathf.Abs(rigidbody.velocity.z)).ToString() + "km/h";
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

    private void FixedUpdate()
    {
        MoveDrone(joystickMovement.inputVector);
        ControlHeightAndRotation(joystickControlHeightAndRotation.inputVector);
        HeightAndSpeedUI();
    }

    private void MoveDrone(Vector2 inputVector)
    {
        Vector3 movementDirection = transform.forward * inputVector.y + transform.right * inputVector.x;
        rigidbody.velocity = movementDirection * moveSpeed;
        if (joystickMovement.isJoystickMovement && !joystickMovement.isJoystickPositionChange() && !isSpeedUpdating)
        {
            StartCoroutine(ChangSpeedWithTime(inputVector));
        }
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

    private IEnumerator ChangSpeedWithTime(Vector2 inputVector)
    {
        float timer = 0;
        float currentSpeed = moveSpeed;
        isSpeedUpdating = true;
        while (timer < timeToModifySpeed)
        {
            if (!joystickMovement.isJoystickMovement)
            {
                moveSpeed = 0;
                isSpeedUpdating = false;
                yield break;
            }
            if (!joystickMovement.isJoystickPositionChange())
            {
                isSpeedUpdating = false;
                yield return new WaitForSeconds(.02f);
                StartCoroutine(ChangSpeedWithTime(inputVector));
                yield break;
            }
            timer += Time.deltaTime;
            moveSpeed = Mathf.Lerp(currentSpeed, moveSpeedDefault*inputVector.magnitude, timer / timeToModifySpeed);
            yield return null;
        }
        isSpeedUpdating = false;
    }
}
