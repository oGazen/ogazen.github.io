using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using URandom = UnityEngine.Random;



public class GameUIMgr : MonoBehaviour
{
    private static GameUIMgr instance;
    public static GameUIMgr Ins => instance;

    private void inside()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }



    // ========================== 层级参数
    [SerializeField] private Canvas m_rootCanvas;
    [SerializeField] private RectTransform m_Panel;
    [SerializeField] private RectTransform m_PopUpPanel;
    [SerializeField] private RectTransform m_TopPanel;

    [SerializeField] private Camera m_mainCamera;





    // ========================== 本地参数
    public const int C_Panel = 1;
    public const int C_Panel_PopUp = 0;
    public const int C_Panel_Top = -1;


    private static int[] s_panels_main = new[] { -1 };
    private static int[] s_panels_game = new[] { -1 };
    private static int[] s_panels_both = new[] { -1 };


    private Dictionary<int, UI_Base> m_panelsDic;
    private Dictionary<int, UnityEvent<UI_Base>> m_panelsEventDic;
    private List<int> m_panelsKey;






    void Awake()
    {
        inside();

        
        m_panelsDic = new Dictionary<int, UI_Base>();
        m_panelsEventDic = new Dictionary<int, UnityEvent<UI_Base>>();
        m_panelsKey = new List<int>();
    }




    public UILoadYield OpenPanel<T>(int ui_key, int panel_key = C_Panel_PopUp,
        UI_Config.Param param = null, UnityAction<UI_Base> callback = null) where T : UI_Base
    {
        
        // 异步迭代器
        UILoadYield uiLoadYield = new UILoadYield(ui_key);

        // Key是否存在
        if (IsHavePanelKey(ui_key))
        {
            // 实例是否存在
            bool ishave = m_panelsDic.TryGetValue(ui_key, out var panel);
            if (ishave)
            {
                callback?.Invoke(panel as T);
                panel.transform.SetAsLastSibling();
            }
            else
            {
                if (callback != null)
                {
                    m_panelsEventDic[ui_key].AddListener(callback as UnityAction<UI_Base>);
                }
            }

            return uiLoadYield;
        }
        else
        {
            m_panelsEventDic.Add(ui_key,new UnityEvent<UI_Base>());
            m_panelsKey.Add(ui_key);
            if (callback != null)
            {
                m_panelsEventDic[ui_key].AddListener(callback);
            }
        }


        
        // 参数检查
        param ??= new UI_Config.Param()
        {
            UI_Key = ui_key,
            IsReleaseWhenClose = false,
        };
        param.UI_Key = ui_key;

        // 开始加载
        Transform tr;
        if (panel_key == 1) tr = m_Panel;
        else if (panel_key == -1) tr = m_TopPanel;
        else tr = m_PopUpPanel;

        StartCoroutine(LoadPanel<T>(ui_key, tr, param));
        return uiLoadYield;
    }


    private IEnumerator LoadPanel<T>(int ui_key, Transform tr_parent, UI_Config.Param param = null) where T : UI_Base
    {
        AssetReference assetReference_ui = GameAssetReferenceMgr.Ins.GetUIPanel(ui_key);
        RALoadMgr.Ins.LoadAsync<GameObject>(assetReference_ui, RALoadType.GameObject, null);
        yield return RALoadMgr.Ins.GetRALoadItemYield(assetReference_ui);

        // 创建面板
        GameObject prefab_ui = RALoadMgr.Ins.Get<GameObject>(assetReference_ui);
        RectTransform rt = Instantiate(prefab_ui, tr_parent).transform as RectTransform;
        rt.localPosition = Vector3.zero;

        T t = rt.GetComponent<T>();
        m_panelsDic.Add(ui_key, t);
        t.Init(param);
        
        m_panelsEventDic[ui_key]?.Invoke(t);
        m_panelsEventDic[ui_key]?.RemoveAllListeners();
    }



    public T GetPanel<T>(int ui_key) where T : UI_Base
    {
        bool ishave = m_panelsDic.TryGetValue(ui_key, out var panel);
        if (ishave)
        {
            return panel as T;
        }
        else
        {
            return null;
        }
    }


    public bool IsHavePanel(int ui_key)
    {
        return m_panelsDic.ContainsKey(ui_key);
    }



    public bool IsHavePanelKey(int ui_key)
    {
        return m_panelsKey.Contains(ui_key);
    }


    public void ClosePanel(int ui_key)
    {
        DConsole.DebugLog($"TEST wgz++++++++++++++++++ ui_key:{ui_key}");
        bool ishave = m_panelsDic.TryGetValue(ui_key, out var panel);
        if (ishave)
        {
            if (panel.ParamBase is { IsReleaseWhenClose: true })
            {
                DConsole.DebugLog($"[GameUIMgr] ClosePanel {nameof(ui_key)} 资源已释放");
                ReleasePanel(ui_key);
            }

            m_panelsKey.Remove(ui_key);
            m_panelsDic.Remove(ui_key);
            m_panelsEventDic.Remove(ui_key);

            Destroy(panel.gameObject);
        }
        else
        {
            DConsole.DebugLog($"[GameUIMgr] ClosePanel 不存在 Key：{ui_key}");
        }
    }






    // ============================= Panel资源释放
    private void ReleasePanel(int ui_key)
    {
        AssetReference assetReference_ui = GameAssetReferenceMgr.Ins.GetUIPanel(ui_key);
        if (assetReference_ui != null)
        {
            RALoadMgr.Ins.ReleaseAddressable(assetReference_ui);
        }
    }


    public void ReleaseMainPanel()
    {
        for (int i = 0; i < s_panels_main.Length; i++)
        {
            var key = s_panels_main[i];
            if (m_panelsDic.ContainsKey(key))
            {
                ReleasePanel(key);
            }
        }
    }


    public void ReleaseGamePanel()
    {
        for (int i = 0; i < s_panels_game.Length; i++)
        {
            var key = s_panels_game[i];
            if (m_panelsDic.ContainsKey(key))
            {
                ReleasePanel(key);
            }
        }
    }


    public void ReleaseBothPanel()
    {
        for (int i = 0; i < s_panels_both.Length; i++)
        {
            var key = s_panels_both[i];
            if (m_panelsDic.ContainsKey(key))
            {
                ReleasePanel(key);
            }
        }
    }


}

