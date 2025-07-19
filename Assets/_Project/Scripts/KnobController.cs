using UnityEngine;
using UnityEngine.EventSystems;

public class KnobController : MonoBehaviour, IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    [Header("References")]
    public RectTransform knobTransform;
    public Paddle controlledPaddle;

    [Header("Rotation Clamp")]
    public float minRotation = -90f;
    public float maxRotation = 90f;

    [Header("Paddle Rotation")]
    public float paddleRotationSpeed = 100f;

    private Vector2 centerPoint;
    private bool isDragging = false;
    private int pointerId = EMPTY;
    
    public static readonly int EMPTY = -999;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        pointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragging || eventData.pointerId != pointerId) return;
        
        // Get center of the knob in screen space
        Vector2 knobCenter = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, knobTransform.position);
        Debug.Log($"drag info is {eventData.position} with knob position {knobCenter}");
        Vector2 pointerPos = eventData.position;

        // Direction from center to pointer
        Vector2 direction = pointerPos - knobCenter;

        // Calculate angle (clockwise from up)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;

        // Clamp the angle
        float clampedAngle = Mathf.Clamp(angle, minRotation, maxRotation);

        // Apply to knob rotation
        knobTransform.localRotation = Quaternion.Euler(0, 0, clampedAngle);

        // // Apply to paddle
        if (controlledPaddle != null)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, clampedAngle);
            controlledPaddle.transform.rotation = Quaternion.RotateTowards(
                controlledPaddle.transform.rotation, targetRotation, paddleRotationSpeed * Time.deltaTime);
        }
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
