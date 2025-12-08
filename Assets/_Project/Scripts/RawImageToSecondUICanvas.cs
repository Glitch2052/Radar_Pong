using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RawImageToSecondUICanvas : MonoBehaviour,
    IInitializePotentialDragHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public RectTransform rawImageTransform;
    public GraphicRaycaster canvasGraphicRayCaster;
    public EventSystem eventSystem;

    // Resolution of the secondary canvas / render texture
    private readonly Vector2 secondaryCanvasResolution = new Vector2(1024, 1024);

    public void OnPointerDown(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.pointerDownHandler);

    public void OnPointerUp(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.pointerUpHandler);

    public void OnPointerClick(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.pointerClickHandler);

    public void OnInitializePotentialDrag(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.initializePotentialDrag);

    public void OnBeginDrag(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.beginDragHandler);

    public void OnDrag(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.dragHandler);

    public void OnEndDrag(PointerEventData eventData) =>
        HandleInputEvent(eventData, ExecuteEvents.endDragHandler);

    private void HandleInputEvent<T>(PointerEventData originalEventData, ExecuteEvents.EventFunction<T> functor)
        where T : IEventSystemHandler
    {
        // 1. Convert screen -> local point in the RawImage
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rawImageTransform,
                originalEventData.position,
                originalEventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        // 2. Local -> normalized UV (0..1)
        Vector2 rawImageSize = rawImageTransform.rect.size;
        Vector2 normalizedUv = new Vector2(
            (localPoint.x + rawImageSize.x * 0.5f) / rawImageSize.x,
            (localPoint.y + rawImageSize.y * 0.5f) / rawImageSize.y
        );

        // 3. UV -> secondary canvas pixel position
        Vector2 renderTexturePos = new Vector2(
            normalizedUv.x * secondaryCanvasResolution.x,
            normalizedUv.y * secondaryCanvasResolution.y
        );

        // 4. Build a "forwarded" PointerEventData
        var pointerData = new PointerEventData(eventSystem)
        {
            position      = renderTexturePos,
            button        = originalEventData.button,
            clickTime     = originalEventData.clickTime,
            clickCount    = originalEventData.clickCount,
            pointerId     = originalEventData.pointerId,
            // For sliders, having pressPosition is very helpful
            pressPosition = renderTexturePos,
            useDragThreshold = originalEventData.useDragThreshold
        };

        // 5. Raycast into the secondary canvas
        List<RaycastResult> results = new List<RaycastResult>();
        canvasGraphicRayCaster.Raycast(pointerData, results);

        if (results.Count == 0)
            return;

        // We usually want only the top-most result, like EventSystem does
        RaycastResult topResult = results[0];
        pointerData.pointerCurrentRaycast = topResult;

        // 6. Execute event on the hit object
        ExecuteEvents.ExecuteHierarchy(topResult.gameObject, pointerData, functor);
    }
}