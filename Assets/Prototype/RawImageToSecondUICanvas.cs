using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RawImageToSecondUICanvas : MonoBehaviour, IInitializePotentialDragHandler, IPointerDownHandler, IDragHandler ,IPointerClickHandler , IPointerUpHandler
{
    public RectTransform rawImageTransform;
    public GraphicRaycaster canvasGraphicRayCaster;
    public EventSystem eventSystem;
    
    private readonly Vector2 secondaryCanvasResolution = new Vector2(1024,1024);

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleInputEvent(eventData,ExecuteEvents.pointerDownHandler);
    }
    
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        HandleInputEvent(eventData,ExecuteEvents.initializePotentialDrag);
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandleInputEvent(eventData,ExecuteEvents.dragHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HandleInputEvent(eventData,ExecuteEvents.pointerUpHandler);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleInputEvent(eventData,ExecuteEvents.pointerClickHandler);
    }

    public void HandleInputEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
    {
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageTransform,eventData.position,null,out Vector2 localPoint))
            return;

        Vector2 rawImageSize = rawImageTransform.rect.size;
        Vector2 normalizedUv = new Vector2(
            (localPoint.x + rawImageSize.x * 0.5f) / rawImageSize.x,
            (localPoint.y + rawImageSize.y * 0.5f) / rawImageSize.y);

        Vector2 renderTexturePos = new Vector2(normalizedUv.x * secondaryCanvasResolution.x,
            normalizedUv.y * secondaryCanvasResolution.y);

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = renderTexturePos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        canvasGraphicRayCaster.Raycast(pointerData, results);

        foreach (RaycastResult result in results)
        {
            ExecuteEvents.Execute(result.gameObject, pointerData, functor);
        }
    }
}
