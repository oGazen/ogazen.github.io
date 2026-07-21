using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.EventSystems;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class newInputTest : MonoBehaviour,IPointerClickHandler
{
    public InputAction inputAction;
    public Texture2D texture2D;

    private void Awake()
    {
        /*
        NewInputSystemTest newTestinput = new NewInputSystemTest();
        newTestinput.FindAction("Fire").performed += NewInputTest_started;
        newTestinput.Enable();
        */

        inputAction.performed += NewInputTest_started1;
        inputAction.Enable();

        EnhancedTouchSupport.Enable();
    }

    private void NewInputTest_started1(InputAction.CallbackContext obj)
    {
        Debug.Log("Wasd1");
    }

    private void NewInputTest_started(InputAction.CallbackContext obj)
    {
        Debug.Log("Wasd");
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        Debug.Log($"this is onclick { context.ReadValue<float>() }");
        Debug.Log($"Touchscreen.current.position {Mouse.current.position.ReadValue()}");
    }
    
    public void OnPoint(InputAction.CallbackContext context)
    {
        Debug.Log($"this is onpoint { context.ReadValue<Vector2>() } name:{context.control.displayName}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnAxis(InputAction.CallbackContext context)
    {
        Debug.Log($"this is OnAxis { context.ReadValue<Vector2>() }");
    }

    public void Clicked()
    {
        Debug.Log($"AAAA {Touchscreen.current.touches.Count} -- {Touch.activeTouches.Count}");
    }
}
