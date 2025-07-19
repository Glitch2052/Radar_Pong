using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventTest : MonoBehaviour
{
    public EventSystem eventSystem;
    public Vector2 inputPos;
    public GraphicRaycaster canvasGraphicRayCaster;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Events Fired");
            
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = inputPos
            };
            
            List<RaycastResult> results = new List<RaycastResult>();
            canvasGraphicRayCaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }
    }

    public void ButtonPressed()
    {
        Debug.Log("Hurray Btn is Pressed");
    }
}
