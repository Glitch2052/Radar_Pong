using UnityEngine;
using UnityEngine.EventSystems;

public class UIKnob : MonoBehaviour, IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    [Header("References")]
    public RectTransform knobTransform;
    public Paddle controlledPaddle;
    
    [Header("Rotation Clamp")]
    public float minRotation = -90f;
    public float maxRotation = 90f;
    
    private bool isDragging = false;
    private bool readyForInput = false;
    private int pointerId = EMPTY;
    public static readonly int EMPTY = -999;

    private Vector2 knobCenter;
    private Vector2 startPos;
    private Vector2 prevRadialPos;
    private Vector2 newRadialPos;

    private float currentRotation;
    private float currVelocity;

    public void Init()
    {
        currentRotation = knobTransform.rotation.eulerAngles.z;
        currentRotation = (currentRotation + 360) % 360;
        readyForInput = true;
    }

    public void IUpdate()
    {
        float dampedZAngle = Mathf.SmoothDampAngle(knobTransform.rotation.eulerAngles.z, currentRotation,
            ref currVelocity, 0.125f,Mathf.Infinity, Time.unscaledDeltaTime);
        knobTransform.rotation = Quaternion.Euler(0,0,dampedZAngle);
        controlledPaddle.transform.rotation = Quaternion.Euler(0, 0, dampedZAngle - 90f);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!readyForInput) return;
        
        isDragging = true;
        pointerId = eventData.pointerId;
        
        knobCenter = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, knobTransform.position);
        startPos = prevRadialPos = newRadialPos = eventData.position;
        newRadialPos = prevRadialPos = startPos - knobCenter;

        currentRotation = knobTransform.rotation.eulerAngles.z;
        currentRotation = (currentRotation + 360) % 360;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragging || eventData.pointerId != pointerId) return;

        knobCenter = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, knobTransform.position);
        newRadialPos = eventData.position - knobCenter;
        float signedDeltaAngle = Vector2.SignedAngle(prevRadialPos, newRadialPos);
        prevRadialPos = newRadialPos;

        currentRotation += signedDeltaAngle * 0.7f;
        currentRotation = ClampAngle(currentRotation, minRotation, maxRotation);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        pointerId = EMPTY;
    }
    
    public float ClampAngle(float angle, float min, float max) {
        float start = (min + max) * 0.5f - 180;
        float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        return Mathf.Clamp(angle, min + floor, max + floor);
    }
}

public enum RotateDirection
{
    ClockWise,
    CounterClockWise
}

public enum PaddleType
{
    None,
    LeftSide,
    RightSide
}