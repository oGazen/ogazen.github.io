using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using UnityEngine.EventSystems;
using URandom = UnityEngine.Random;


public class DInputUIMgr : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler,IBeginDragHandler,IEndDragHandler,IDragHandler
{
    private static DInputUIMgr instance;
    public static DInputUIMgr Ins => instance;


    
    // ==================== 本地变量
    private DInputData m_inputData;
    private List<IInputBase> m_interfaceInputBaseList;

    private bool m_isPointerDown;


    private void inside()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    void Awake()
    {
        // 单点触控
        Input.multiTouchEnabled = false;
        inside();
        m_inputData = default;

        m_interfaceInputBaseList = new List<IInputBase>(4);
    }


    private void OnDestroy()
    {
        m_interfaceInputBaseList.Clear();
    }

    



    public void Register(IInputBase inputBase)
    {
        bool ishave = m_interfaceInputBaseList.Contains(inputBase);
        DConsole.Asset(!ishave,$"[DInputUIMgr] >> Register 注册实例已存在，请检查");
        if (!ishave)
        {
            m_interfaceInputBaseList.Add(inputBase);
        }
    }



    public void UnRegister(IInputBase inputBase)
    {
        bool ishave = m_interfaceInputBaseList.Contains(inputBase);
        if (ishave)
        {
            m_interfaceInputBaseList.Remove(inputBase);
            return;
        }
        DConsole.DebugWarning("[DInputUIMgr] >> UnRegister 注册实例不存在，请检查");
    }
    
    
    
    

    private void SetInputData(PointerEventData eventData)
    {
        m_inputData.positon = eventData.position;
        m_inputData.pressPosition = eventData.pressPosition;
        m_inputData.delta = eventData.delta;
        m_inputData.dragging = eventData.dragging;
    }
    
    

    public void OnPointerDown(PointerEventData eventData)
    {
        // 微信多点触控屏蔽测试
        if (m_isPointerDown)
        {
            return;
        }

        m_isPointerDown = true;
        SetInputData(eventData);
        foreach (var item in m_interfaceInputBaseList)
        {
            if (!item.IsInputEnabled) continue;
            item.OnPointerDown(m_inputData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_isPointerDown = false;
        
        SetInputData(eventData);
        foreach (var item in m_interfaceInputBaseList)
        {
            if (!item.IsInputEnabled) continue;
            item.OnPointerUp(m_inputData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetInputData(eventData);
        foreach (var item in m_interfaceInputBaseList)
        {
            if (!item.IsInputEnabled) continue;
            Vector2 offsetInput = m_inputData.positon - m_inputData.pressPosition;
            bool isCan = offsetInput.magnitude <= 16f;
            if(isCan)
                item.OnPointerClick(m_inputData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        SetInputData(eventData);
        foreach (var item in m_interfaceInputBaseList)
        {
            if (!item.IsInputEnabled) continue;
            item.OnBeginDrag(m_inputData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SetInputData(eventData);
        foreach (var item in m_interfaceInputBaseList)
        {
            if (!item.IsInputEnabled) continue;
            item.OnEndDrag(m_inputData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetInputData(eventData);
        foreach (var item in m_interfaceInputBaseList)
        {
            if (!item.IsInputEnabled) continue;
            item.OnDrag(m_inputData);
        }
    }
    
    
    
    
    
}

