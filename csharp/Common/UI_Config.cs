using System;
using UnityEngine;
using URandom = UnityEngine.Random;




public class UI_Config
{
    public class Param
    {
        public int UI_Key = -1;
        public bool IsReleaseWhenClose = false;
    }
    
    
}


public class UI_Base : MonoBehaviour
{
    public UI_Config.Param ParamBase;


    public virtual void Close()
    {
        DConsole.DebugLog($"TEST wgz++++++++++++++++++++++ GameUIMgr.Ins:{GameUIMgr.Ins}");
        GameUIMgr.Ins.ClosePanel(ParamBase.UI_Key);
    }


    public virtual void Init(UI_Config.Param param)
    {
        ParamBase = param;
    }
}


    
public class UILoadYield: CustomYieldInstruction
{
    public override bool keepWaiting => !GameUIMgr.Ins.IsHavePanel(UI_Key);

    private int UI_Key;
    public UILoadYield(int ui_key)
    {
        UI_Key = ui_key;
    }
}