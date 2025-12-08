using System.Collections.Generic;
using System.Linq;
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
    private float lastDampedAngle;

    public float AngularVelocity
    {
        get
        {
            float velocity = angularVelocities.Average();
            velocity = Mathf.Clamp(velocity, -90, 90);
// #if UNITY_EDITOR
//             Debug.Log($"Angular Velocity is {angularVelocities.Average()} and clamped is {velocity}");
// #endif
            return velocity;
        }
    }

    private readonly List<float> angularVelocities = new();
    // private readonly List<Vector2> velocities = new ();
    // public Vector2 Velocity
    // {
    //     get
    //     {
    //         Vector2 sum = Vector2.zero;
    //         foreach (var v in velocities)
    //             sum += v;
    //         return sum / velocities.Count;
    //     }
    // }

    public static readonly float DefaultSensitivity = 0.5f;
    public static readonly float MinSensitivity = 0.25f;
    public static readonly float MaxSensitivity = 0.75f;
    private static float sensitivity;
    public static float Sensitivity
    {
        get => sensitivity;
        set
        {
            sensitivity = value;
            PlayerPrefs.SetFloat(StringID.KnobSensitivity,sensitivity);
            PlayerPrefs.Save();
        }
    }

    public void Init()
    {
        currentRotation = knobTransform.rotation.eulerAngles.z;
        currentRotation = (currentRotation + 360) % 360;
        readyForInput = true;
        
        controlledPaddle.Init(this);
    }

    public void IUpdate()
    {
        float dampedZAngle = Mathf.SmoothDampAngle(knobTransform.rotation.eulerAngles.z, currentRotation,
            ref currVelocity, 0.125f,Mathf.Infinity, Time.unscaledDeltaTime);
        knobTransform.rotation = Quaternion.Euler(0,0,dampedZAngle);
        controlledPaddle.transform.rotation = Quaternion.Euler(0, 0, dampedZAngle - 90f);

        float dt = Time.unscaledDeltaTime;
        if (dt > 0)
        {
            float deltaAngle = Mathf.DeltaAngle(lastDampedAngle, dampedZAngle);
            AddAngularVelocity(deltaAngle / dt);
        }
        lastDampedAngle = dampedZAngle;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!readyForInput || GameManager.instance.IsPaused) return;
        
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

        currentRotation += signedDeltaAngle * Sensitivity;
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

    public static void UpdateKnobSensitivity(float lerpValue)
    {
        Sensitivity = Mathf.Lerp(MinSensitivity, MaxSensitivity,lerpValue);
    }

    // private void AddVelocity(Vector2 value)
    // {
    //     if (velocities.Count > 5) velocities.RemoveAt(0);
    //     velocities.Add(value);
    // }
    
    private void AddAngularVelocity(float value)
    {
        if (angularVelocities.Count > 4) angularVelocities.RemoveAt(0);
        angularVelocities.Add(value);
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