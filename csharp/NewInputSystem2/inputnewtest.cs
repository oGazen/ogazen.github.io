using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.EnhancedTouch;

using TouchEnhanced = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Linq;

public class inputnewtest : MonoBehaviour
{
    [SerializeField] private InputAction m_firstAction;

    // UI Canvas
    [SerializeField] private GameObject m_ui_canvas;

    // Local
    private GraphicRaycaster m_ui_graphicRaycaster;
    private PointerEventData m_ui_pointerEventData;
    private List<RaycastResult> m_ui_raycastResults;

    private bool m_isoverUI = false;






    // Start is called before the first frame update
    void Start()
    {
        m_firstAction.performed += onPerormed;
        m_firstAction.started += onStarted;
        m_firstAction.canceled += onCanceled;
        m_firstAction.Enable();

        m_ui_graphicRaycaster = m_ui_canvas.GetComponent<GraphicRaycaster>();
        m_ui_pointerEventData = new PointerEventData(EventSystem.current);
        m_ui_raycastResults = new List<RaycastResult>();
    }



    private void OnEnable()
    {
        var a = InputSystem.onEvent;
        a += customHandle;
    }

    private void OnDisable()
    {
        var a = InputSystem.onEvent;
        a -= customHandle;
    }


    private unsafe void customHandle(InputEventPtr inputEventPtr, InputDevice device)
    {


        //if (m_isoverUI)
        //{
        //    //inputEventPtr.handled = true;
        //    m_isoverUI = false;
        //    Debug.Log("wgz++++ customHandle >> 阻止事件传播");
        //    Debug.Log($"wgz++++ inputEventPtr:{inputEventPtr} device:{device}");


        //    var c = inputEventPtr.EnumerateChangedControls().ToArray();
        //}


    }






    // Update is called once per frame
    void Update()
    {
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    Debug.Log("IsPointerOverGameObject >> true");
        //}

        //if (Mouse.current.leftButton.wasPressedThisFrame)
        //{
        //    m_ui_pointerEventData.position = Mouse.current.position.ReadValue();
        //    m_ui_raycastResults.Clear();
        //    m_ui_graphicRaycaster.Raycast(m_ui_pointerEventData, m_ui_raycastResults);


        //    //foreach (RaycastResult result in m_ui_raycastResults)
        //    //{
        //    //    GameObject ui_element = result.gameObject;
        //    //}

        //    m_isoverUI = m_ui_raycastResults.Count > 0;
        //    Debug.Log($"wgz++++ wasPressed >> m_isoverUI:{m_isoverUI}");
        //}

        //if (Mouse.current.leftButton.wasReleasedThisFrame)
        //{
        //    Debug.Log($"wgz++++ wasReleasedThisFrame >> m_isoverUI:{m_isoverUI}");
        //}


    }


    private void onPerormed(InputAction.CallbackContext callbackContext)
    {
        var type = callbackContext.valueType;
        Debug.Log($"var inputaction onPerormed type:{type} {callbackContext.ReadValue<float>()}");

    }


    private void onCanceled(InputAction.CallbackContext context)
    {
        Debug.Log($"var inputaction onCanceled");
    }

    private void onStarted(InputAction.CallbackContext context)
    {
        Debug.Log($"var inputaction onStarted");
    }



}
