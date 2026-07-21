using System;
using System.Collections.Generic;
using UnityEngine.Events;
using URandom = UnityEngine.Random;



public class DEventManager
{
    public static DEventManager Ins = new DEventManager();
    private Dictionary<int, DEventOrigin> EventBaseDic;


    public const int E_OnEnterBackGround = 0; // 当切到后台时 常用于保存游戏
    public const int E_OnEnterFrontGround = 1; // 当切到前台时

    
    
    private DEventManager()
    {
        EventBaseDic = new Dictionary<int, DEventOrigin>();
        EventBaseDic.Add(E_OnEnterBackGround,new DEventOrigin());
        EventBaseDic.Add(E_OnEnterFrontGround,new DEventOrigin());
        
    }

    


    



    public void Register(int Key_EventID,UnityAction unityAction)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] Register 事件未被初始化，请检查");
        orgin.Register(unityAction);
    }
    
    public void UnRegister(int Key_EventID,UnityAction unityAction)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] UnRegister 事件未被初始化，请检查");
        orgin.UnRegister(unityAction);
    }
    
    public void Register<T>(int Key_EventID,UnityAction<T> unityAction)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] Register 事件未被初始化，请检查");

        DEventBase<T> eventBase = orgin as DEventBase<T>;
        DConsole.Asset(eventBase != null,"[DEventManager] Register 事件类型参数，请检查");
        eventBase.Register(unityAction);
    }
    
    public void UnRegister<T>(int Key_EventID,UnityAction<T> unityAction)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] UnRegister 事件未被初始化，请检查");

        DEventBase<T> eventBase = orgin as DEventBase<T>;
        DConsole.Asset(eventBase != null,"[DEventManager] UnRegister 事件类型参数，请检查");
        eventBase.UnRegister(unityAction);
    }

    
    public void Register<T,S>(int Key_EventID,UnityAction<T,S> unityAction)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] Register 事件未被初始化，请检查");

        DEventBase<T,S> eventBase = orgin as DEventBase<T,S>;
        DConsole.Asset(eventBase != null,"[DEventManager] Register 事件类型参数，请检查");
        eventBase.Register(unityAction);
    }
    
    public void UnRegister<T,S>(int Key_EventID,UnityAction<T,S> unityAction)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] UnRegister 事件未被初始化，请检查");

        DEventBase<T,S> eventBase = orgin as DEventBase<T,S>;
        DConsole.Asset(eventBase != null,"[DEventManager] UNRegister 事件类型参数，请检查");
        eventBase.UnRegister(unityAction);
    }

    public void Execute(int Key_EventID)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] Execute 事件未被初始化，请检查");
        orgin.Execute();
    }
    
    public void Execute<T>(int Key_EventID,T value)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] Execute 事件未被初始化，请检查");
        
        DEventBase<T> eventBase = orgin as DEventBase<T>;
        DConsole.Asset(eventBase != null,"[DEventManager] Execute 事件类型参数，请检查");
        eventBase.Execute(value);
    }
    
    public void Execute<T,S>(int Key_EventID,T value,S value2)
    {
        bool ishave = EventBaseDic.TryGetValue(Key_EventID, out DEventOrigin orgin);
        DConsole.Asset(ishave,"[DEventManager] Execute 事件未被初始化，请检查");
        
        DEventBase<T,S> eventBase = orgin as DEventBase<T,S>;
        DConsole.Asset(eventBase != null,"[DEventManager] Execute 事件类型参数，请检查");
        eventBase.Execute(value,value2);
    }
}


public class DEventBase<T> : DEventOrigin
{
    public UnityAction<T> OnAction;
    
    public void Execute(T value)
    {
        OnAction?.Invoke(value);
    }

    public void Register(UnityAction<T> unityAction)
    {
        OnAction += unityAction;
    }

    public void UnRegister(UnityAction<T> unityAction)
    {
        OnAction -= unityAction;
    }
}


public class DEventBase<T,S> : DEventOrigin
{
    public UnityAction<T,S> OnAction;
    
    public void Execute(T value,S value2)
    {
        OnAction?.Invoke(value,value2);
    }

    public void Register(UnityAction<T,S> unityAction)
    {
        OnAction += unityAction;
    }

    public void UnRegister(UnityAction<T,S> unityAction)
    {
        OnAction -= unityAction;
    }
}


public class DEventOrigin
{
    public UnityAction OnAction;
    
    public void Execute()
    {
        OnAction?.Invoke();
    }

    public void Register(UnityAction unityAction)
    {
        OnAction += unityAction;
    }

    public void UnRegister(UnityAction unityAction)
    {
        OnAction -= unityAction;
    }
}
