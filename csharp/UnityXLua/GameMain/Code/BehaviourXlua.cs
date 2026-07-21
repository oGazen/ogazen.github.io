using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using XLua;


[System.Serializable]
public class Injection
{
    public string name;
    public GameObject value;
}


public class BehaviourXlua : MonoBehaviour
{
    private static LuaEnv luaEnv;
    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;
    private static byte[] LoaderMe(ref string Luafile_path_local)
    {
        var path = $"{CommonTool.GetBaseAssetPath()}/LuaScripts/{Luafile_path_local}.lua.txt";
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        return null;
    }


    private static void LoadIntelliSenseCode()
    {
        var path = $"{CommonTool.GetBaseAssetPath()}/LuaScripts/IntelliSenseCode";
        var _assembly_lua = $"{path}/_Assembly_Lua.lua.txt";
        luaEnv.DoString(File.ReadAllBytes(_assembly_lua),_assembly_lua);
        
        DirectoryInfo directory = Directory.CreateDirectory(path); 
        FileInfo[] files = directory.GetFiles("CS*.lua.txt",SearchOption.AllDirectories);
        foreach (var file in files)
        {
            luaEnv.DoString(File.ReadAllBytes(file.FullName),file.FullName);
        }
    }
    
    
    
    [SerializeField] private string m_Luafile;
    [SerializeField] private Injection[] m_injections;
    
    
    private LuaTable m_scriptScopeTable;
    private Action luaStart;
    private Action luaOnEnbale;
    private Action luaOnDisable;
    private Action luaUpdate;
    private Action luaOnDestroy;
    
    
    private void Awake()
    {
        // -1
        if (luaEnv == null)
        {
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(LoaderMe);
            LoadIntelliSenseCode();
        }
        if(string.IsNullOrEmpty(m_Luafile) || LoaderMe(ref m_Luafile)==null) return;
        
        
        
        
        // 0
        var file_name = m_Luafile.Replace('/','_');
        
        
        // 1
        m_scriptScopeTable = luaEnv.NewTable();
        using (LuaTable meta = luaEnv.NewTable())
        {
            meta.Set("__index", luaEnv.Global);
            m_scriptScopeTable.SetMetaTable(meta);
        }
        
        
        // 2
        m_scriptScopeTable.Set("self", this);
        m_scriptScopeTable.Set("Injections", luaEnv.NewTable());
        foreach (var injection in m_injections)
        {
            m_scriptScopeTable.SetInPath($"Injections.{injection.name}" , injection.value);
        }
        
        //3
        luaEnv.DoString(LoaderMe(ref m_Luafile), m_Luafile, m_scriptScopeTable);
        var table = m_scriptScopeTable.Get<LuaTable>($"{file_name}");
        m_scriptScopeTable.Set("this", table);
        
        
        luaStart = m_scriptScopeTable.GetInPath<Action>($"{file_name}.start");
        luaUpdate = m_scriptScopeTable.GetInPath<Action>($"{file_name}.update");
        luaOnDisable = m_scriptScopeTable.GetInPath<Action>($"{file_name}.ondisable");
        luaOnEnbale = m_scriptScopeTable.GetInPath<Action>($"{file_name}.onenable");
        luaOnDestroy = m_scriptScopeTable.GetInPath<Action>($"{file_name}.destroy");
        
        Action awake = m_scriptScopeTable.GetInPath<Action>($"{file_name}.awake");
        awake?.Invoke();
    }
    
    
    


    private void OnEnable()
    {
        luaOnEnbale?.Invoke();
    }

    private void OnDisable()
    {
        luaOnDisable?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        luaStart?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        luaUpdate?.Invoke();
        
        if (Time.time - BehaviourXlua.lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            BehaviourXlua.lastGCTime = Time.time;
        }
    }


    private void OnDestroy()
    {
        luaOnDestroy?.Invoke();
        
        m_scriptScopeTable.Dispose();
        m_injections = null;
        
        luaStart = null;
        luaUpdate = null;
        luaOnDestroy = null;
        luaOnEnbale = null;
        luaOnDisable = null;
    }
}
