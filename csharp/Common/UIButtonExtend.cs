using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using URandom = UnityEngine.Random;



public class UIButtonExtend : Button
{

    private enum State
    {
        None,
        PointerDown,
        PointerUp,
        HoldDown,
        Hold,
        HoldUp,
    }
    
    
    
    
    // ================ 本地变量
    private const float C_DoubleClick_Interval = 0.29f;
    private const float C_HoldDown_Time = 0.3f;


    private State m_state;
    private int m_clickCount;
    private float m_timeClickFirst;

    private float m_timeHold;

    
    // ================= 事件
    [Serializable] public class ButtonDoubleClickedEvent : UnityEvent {}
    [Serializable] public class ButtonHoldDownEvent : UnityEvent {}
    [Serializable] public class ButtonHoldUpEvent : UnityEvent {}

    
    [SerializeField]private ButtonDoubleClickedEvent m_OnDoubleClick = new ButtonDoubleClickedEvent();
    [SerializeField]private ButtonHoldDownEvent m_HoldDown = new ButtonHoldDownEvent();
    [SerializeField]private ButtonHoldUpEvent m_HoldUp = new ButtonHoldUpEvent();
    

    public ButtonDoubleClickedEvent onDoubleClick
    {
        get { return m_OnDoubleClick; }
        set { m_OnDoubleClick = value; }
    }
    
    public ButtonHoldDownEvent onHoldDown
    {
        get { return m_HoldDown; }
        set { m_HoldDown = value; }
    }
    
    public ButtonHoldUpEvent onHoldUp
    {
        get { return m_HoldUp; }
        set { m_HoldUp = value; }
    }
    
    
    
    
    
    
    
    protected override void Awake()
    {
        base.Awake();
        m_state = State.None;
    }


    private void Update()
    {
        // === 长按
        if (m_state == State.PointerDown)
        {
            if (Time.time - m_timeHold >= C_HoldDown_Time)
            {
                m_state = State.HoldDown;
            }
        }
        else if(m_state == State.HoldDown)
        {
            DConsole.DebugLog("[UIButtonExtend] >> Update  进入长按事件");
            m_HoldDown?.Invoke();
            m_state = State.Hold;
        }
        else if(m_state == State.HoldUp)
        {
            DConsole.DebugLog("[UIButtonExtend] >> Update 取消长按事件");
            m_HoldUp?.Invoke();
            m_state = State.None;
        }
    }


    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        m_state = State.PointerDown;

        
        // === 双击
        if (m_clickCount == 0 || Time.time - m_timeClickFirst <= C_DoubleClick_Interval)
        {
            m_clickCount += 1;
        }
        
        if (m_clickCount == 1)
        {
            m_timeClickFirst = Time.time;
        }
        
        // === 长按
        m_timeHold = Time.time;
    }


    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        m_state = m_state == State.Hold ? State.HoldUp : State.PointerUp;
        

        // === 双击
        float interval_current = Time.time - m_timeClickFirst;
        if (m_clickCount == 2 && interval_current <= C_DoubleClick_Interval)
        {
            DConsole.DebugLog($"[UIButtonExtend] >> OnPointerUp 触发双击");
            m_OnDoubleClick?.Invoke();
            m_clickCount = 0;
        }

        if (interval_current > C_DoubleClick_Interval)
        {
            m_clickCount = 0;
        }
        
    }
    
    
    
    
}
