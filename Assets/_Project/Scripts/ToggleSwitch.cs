using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Slider setup")] 
    [SerializeField] Slider slider;
    [SerializeField, Range(0, 1f)]
    protected float sliderValue;
    public bool CurrentValue { get; private set; }
    
    private bool previousValue;

    [Header("Animation")] 
    [SerializeField, Range(0, 1f)] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve slideEase =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine animateSliderCoroutine;

    [Header("Events")] 
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;
    
    protected event Action TransitionEffect;

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }
    
    private void Toggle()
    {
        SetStateAndStartAnimation(!CurrentValue);
    }
    
    private void SetStateAndStartAnimation(bool state)
    {
        previousValue = CurrentValue;
        CurrentValue = state;

        if (previousValue != CurrentValue)
        {
            if (CurrentValue)
                onToggleOn?.Invoke();
            else
                onToggleOff?.Invoke();
        }

        if (animateSliderCoroutine != null)
            StopCoroutine(animateSliderCoroutine);

        animateSliderCoroutine = StartCoroutine(AnimateSlider());
    }


    private IEnumerator AnimateSlider()
    {
        float startValue = slider.value;
        float endValue = CurrentValue ? 1 : 0;

        float time = 0;
        if (animationDuration > 0)
        {
            while (time < animationDuration)
            {
                time += Time.deltaTime;

                float lerpFactor = slideEase.Evaluate(time / animationDuration);
                slider.value = sliderValue = Mathf.Lerp(startValue, endValue, lerpFactor);

                TransitionEffect?.Invoke();
                    
                yield return null;
            }
        }

        slider.value = endValue;
    }
}