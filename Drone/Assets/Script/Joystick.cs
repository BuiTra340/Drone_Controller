using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]private Image backgroundImg;
    [SerializeField] private Image handleImg;
    public bool isJoystickMovement { get; private set; }
    public Vector2 inputVector { get;private set; }
    private Vector2 previousHandlePosition;

    public void OnDrag(PointerEventData ped)
    {
        isJoystickMovement = true;
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundImg.rectTransform,
            ped.position,
            ped.pressEventCamera,
            out pos))
        {
            pos.x = (pos.x / backgroundImg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / backgroundImg.rectTransform.sizeDelta.y);

            inputVector = new Vector2(pos.x * 2 - 1, pos.y * 2 - 1);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            handleImg.rectTransform.anchoredPosition = new Vector2(
                inputVector.x * (backgroundImg.rectTransform.sizeDelta.x / 2),
                inputVector.y * (backgroundImg.rectTransform.sizeDelta.y / 2));
            StartCoroutine(CheckDragPosition());
        }
    }

    public void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public void OnPointerUp(PointerEventData ped)
    {
        inputVector = Vector2.zero;
        handleImg.rectTransform.anchoredPosition = Vector2.zero;
        isJoystickMovement = false;
    }

    public bool isJoystickPositionNotChange()
    {
        return previousHandlePosition == handleImg.rectTransform.anchoredPosition;
    }

    private IEnumerator CheckDragPosition()
    {
        yield return new WaitForSeconds(.02f);
        previousHandlePosition = handleImg.rectTransform.anchoredPosition;
    }
}