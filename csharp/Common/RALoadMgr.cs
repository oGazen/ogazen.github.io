using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using URandom = UnityEngine.Random;


public class RALoadMgr : MonoBehaviour
{
    private static RALoadMgr instance;
    public static RALoadMgr Ins => instance;

    
    private Dictionary<string, RALoadItem> m_ResDic;
    private Dictionary<string, RALoadItem> m_AddressableDic;
    private JsonData m_addressableKeyMap;
    


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
        inside();
        m_ResDic = new Dictionary<string, RALoadItem>();
        m_AddressableDic = new Dictionary<string, RALoadItem>();
        m_addressableKeyMap = JsonMapper.ToObject<JsonData>(Resources.Load<TextAsset>("Config/Other/AddressableKeyMap").text);
    }

    
    
    
    
    
    // ================== Load Resources ==================
    // T 类型仅属于 RaLoadType 
    public void LoadResAsync<T>(string fileRelativePath,RALoadType loadType,UnityAction<T> callback) where T : UnityEngine.Object
    {
        // 检查
        bool ishave = m_ResDic.TryGetValue(fileRelativePath, out RALoadItem item);
        if (ishave)
        {
            if (item.RALoadStatus == RALoadStatus.Loading)
            {
                item.ResourceRequestResult.completed += (AsyncOperation AO) =>
                {
                    DConsole.Asset(item.ResourceRequestResult.asset != null,$"[RALoadMgr] LoadResAsync {fileRelativePath} 路径文件不存在,请检查");
                    callback?.Invoke(item.ResourceRequestResult.asset as T);
                };
            }
            else
            {
                callback?.Invoke(item.ResourceRequestResult.asset as T);
            }
            return;
        }
        
        
        // 添加字典
        RALoadItem newitem = new RALoadItem();
        newitem.Key = fileRelativePath;
        newitem.RALoadMode = RALoadMode.Resources;
        newitem.RALoadType = loadType;
            
        ResourceRequest resourceRequest = Resources.LoadAsync<T>(fileRelativePath);
        DConsole.Asset(resourceRequest != null,$"[RALoadMgr] LoadResAsync {fileRelativePath} 不存在");
        newitem.RALoadStatus = RALoadStatus.Loading;
        newitem.ResourceRequestResult = resourceRequest;
        
        DConsole.DebugLog($"[RALoadMgr] LoadResAsync 加载 {fileRelativePath}");
        m_ResDic.Add(fileRelativePath,newitem);
        resourceRequest.completed += (AsyncOperation AO) =>
        {
            DConsole.Asset(resourceRequest.asset != null,$"[RALoadMgr] LoadResAsync {fileRelativePath} 路径文件不存在,请检查");
            newitem.RALoadStatus = RALoadStatus.Loaded;
            callback?.Invoke(resourceRequest.asset as T);
        };
    }
    
    

    
    // ================== Load Addressable ================
    public void LoadAsync<T>(string fileRelativePath, RALoadType loadType, UnityAction<T> callback) where T : UnityEngine.Object
    {
        // 检查
        bool ishave = m_AddressableDic.TryGetValue(fileRelativePath, out RALoadItem item);
        if (ishave)
        {
            if (item.RALoadStatus == RALoadStatus.Loading)
            {
                AddressableRegisterCallback(item,callback);
            }
            else
            {
                callback?.Invoke(item.AsyncOperationHandle.Result as T);
            }
            return;
        }

        
        RALoadItem raLoadItem = new RALoadItem();
        AsyncOperationHandle asyncOperationHandle = Addressables.LoadAssetAsync<T>(fileRelativePath);
        DConsole.Asset(asyncOperationHandle.IsValid(),$"[RALoadMgr] LoadAsync {fileRelativePath} 无效,请检查");
        
        
        raLoadItem.AsyncOperationHandle = asyncOperationHandle;
        raLoadItem.RALoadType = loadType;
        raLoadItem.RALoadStatus = RALoadStatus.Loading;
        raLoadItem.RALoadMode = RALoadMode.Addressable;
        raLoadItem.Key = fileRelativePath;
        
        DConsole.DebugLog($"[RALoadMgr] LoadAsync 加载 {fileRelativePath}");
        m_AddressableDic.Add(fileRelativePath,raLoadItem);
        AddressableRegisterCallback(raLoadItem,callback); 
    }



    public void LoadAsync<T>(AssetReference assetReference, RALoadType loadType, UnityAction<T> callback) where T : UnityEngine.Object
    {
        string key = assetReference.RuntimeKey.ToString();
        string path = null;
        if (key.IndexOf('[') == -1)
        {
            path = m_addressableKeyMap[key].ToString();
        }
        else
        {
            Match match = Regex.Match(key, "^[A-Za-z0-9]+");
            DConsole.Asset(match.Success,$"[RALoadMgr] LoadAsync 无效的匹配");
            path = m_addressableKeyMap[match.Value].ToString();
        }
        DConsole.Asset(!String.IsNullOrEmpty(path),$"[RALoadMgr] LoadAsync {assetReference}:{path} 无效的参数，请检查 AddressableKeyMap.json 配置");
        
        LoadAsync(path,loadType,callback);
    }
    


    private void AddressableRegisterCallback<T>(RALoadItem raLoadItem,UnityAction<T> unityAction) where T : UnityEngine.Object
    {
        if (raLoadItem.RALoadType == RALoadType.Sprite)
        {
            raLoadItem.AsyncOperationHandle.Completed += (handler) =>
            {
                raLoadItem.RALoadStatus = RALoadStatus.Loaded;
                Texture2D texture2D = handler.Result as Texture2D;
                Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(texture2D.width / 2, texture2D.height / 2));
                raLoadItem.Sprite = sprite;
                unityAction?.Invoke(sprite as T);
            };
        }
        else
        {
            raLoadItem.AsyncOperationHandle.Completed += (handler) =>
            {
                raLoadItem.RALoadStatus = RALoadStatus.Loaded;
                T tt = handler.Result as T;
                unityAction?.Invoke(tt);
            };
        }
    }






    public void ReleaseResources(string fileRelativePath)
    {
        bool ishave = m_ResDic.TryGetValue(fileRelativePath, out RALoadItem item);
        DConsole.Asset(ishave,$"[RALoadMgr] ReleaseResources {fileRelativePath} 无效，请检查");
        
        m_ResDic.Remove(fileRelativePath);
        item = null;
    }



    public void ReleaseAddressable(string fileRelativePath)
    {
        bool ishave = m_AddressableDic.TryGetValue(fileRelativePath, out RALoadItem item);
        DConsole.Asset(ishave,$"[RALoadMgr] ReleaseAddressable {fileRelativePath} 无效，请检查");

        if (ishave)
        {
            Addressables.Release(item.AsyncOperationHandle);
            m_AddressableDic.Remove(fileRelativePath);
        }
    }


    public void ReleaseAddressable(AssetReference assetReference)
    {
        string key = assetReference.RuntimeKey.ToString();
        string path = null;
        if (key.IndexOf('[') == -1)
        {
            path = m_addressableKeyMap[key].ToString();
        }
        else
        {
            Match match = Regex.Match(key, "^[A-Za-z0-9]+");
            DConsole.Asset(match.Success,$"[RALoadMgr] ReleaseAddressable 无效AssetReference参数");
            path = m_addressableKeyMap[match.Value].ToString();
        }
        
        DConsole.Asset(!String.IsNullOrEmpty(path),$"[RALoadMgr] ReleaseAddressable  解析的Path:{path} 无效，请检查");
        ReleaseAddressable(path);
    }
    
    
    
    
    
    // =================== 协程与同步操作 ==================
    public RALoadItemYield GetRALoadItemYield(string fileRelativePath)
    {
        bool ishave_res = m_ResDic.TryGetValue(fileRelativePath, out RALoadItem raLoadItem_res);
        if (ishave_res)
        {
            return raLoadItem_res as RALoadItemYield;
        }

        bool ishave_addressable = m_AddressableDic.TryGetValue(fileRelativePath, out RALoadItem raLoadItem);
        if (ishave_addressable)
        {
            return raLoadItem as RALoadItemYield;
        }

        
        DConsole.Asset(ishave_res || ishave_addressable,$"[RALoadMgr] GetRALoadItemYield {fileRelativePath} 不存在，请检查");
        return null;
    }


    public RALoadItemYield GetRALoadItemYield(AssetReference assetReference)
    {
        string key = assetReference.RuntimeKey.ToString();
        string path = null;
        if (key.IndexOf('[') == -1)
        {
            path = m_addressableKeyMap[key].ToString();
        }
        else
        {
            Match match = Regex.Match(key, "^[A-Za-z0-9]+");
            DConsole.Asset(match.Success,$"[RALoadMgr] GetRALoadItemYield 无效的匹配");
            path = m_addressableKeyMap[match.Value].ToString();
        }
        DConsole.Asset(!String.IsNullOrEmpty(path),$"[RALoadMgr] GetRALoadItemYield {assetReference}:{path} 无效的参数，请检查 AddressableKeyMap.json 配置");

        return GetRALoadItemYield(path);
    }

    
    public T Get<T>(string fileRelativePath)  where T : UnityEngine.Object
    {
        bool ishave_res = m_ResDic.TryGetValue(fileRelativePath, out RALoadItem raLoadItem_res);
        if (ishave_res)
        {
            return raLoadItem_res.ResourceRequestResult.asset as T;
        }

        bool ishave_addressable = m_AddressableDic.TryGetValue(fileRelativePath, out RALoadItem raLoadItem);
        if (ishave_addressable)
        {
            return raLoadItem.AsyncOperationHandle.Result as T;
        }
        
        DConsole.Asset(ishave_res || ishave_addressable,$"[RALoadMgr] Get {fileRelativePath} 不存在，请检查");
        return null;
    }
    
    
    
    public T Get<T>(AssetReference assetReference)  where T : UnityEngine.Object
    {
        string key = assetReference.RuntimeKey.ToString();
        string path = null;
        if (key.IndexOf('[') == -1)
        {
            path = m_addressableKeyMap[key].ToString();
        }
        else
        {
            Match match = Regex.Match(key, "^[A-Za-z0-9]+");
            DConsole.Asset(match.Success,$"[RALoadMgr] Get 无效的匹配");
            path = m_addressableKeyMap[match.Value].ToString();
        }
        DConsole.Asset(!String.IsNullOrEmpty(path),$"[RALoadMgr] Get {assetReference}:{path} 无效的参数，请检查 AddressableKeyMap.json 配置");

        return Get<T>(path);
    }


    
    public void GetAction<T>(string fileRelativePath,UnityAction<T> callback) where T : UnityEngine.Object
    {
        // 已加载完成
        RALoadItemYield raLoadItemYield_file = GetRALoadItemYield(fileRelativePath);
        if (raLoadItemYield_file is { keepWaiting: false })
        {
            callback?.Invoke(Get<T>(fileRelativePath));
            return;
        }
        
        // 未加载完成功能
        bool ishave_res = m_ResDic.TryGetValue(fileRelativePath, out RALoadItem raLoadItem_res);
        if (ishave_res)
        {
            raLoadItem_res.ResourceRequestResult.completed += (AsyncOperation asyncOperation) =>
            {
                callback?.Invoke(raLoadItem_res.ResourceRequestResult.asset as T);
            };
            return;
        }

        bool ishave_addressable = m_AddressableDic.TryGetValue(fileRelativePath, out RALoadItem raLoadItem);
        if (ishave_addressable)
        {
            raLoadItem.AsyncOperationHandle.Completed += (AsyncOperationHandle asyncOperationHandle) =>
            {
                callback?.Invoke(raLoadItem.AsyncOperationHandle.Result as T);
            };
            return;
        }
        
        DConsole.Asset(ishave_res || ishave_addressable,$"[RALoadMgr] GetAction {fileRelativePath} 不存在，请检查");
    }




    public void GetAction<T>(AssetReference assetReference, UnityAction<T> callback) where T : UnityEngine.Object
    {
        string key = assetReference.RuntimeKey.ToString();
        string path = null;
        if (key.IndexOf('[') == -1)
        {
            path = m_addressableKeyMap[key].ToString();
        }
        else
        {
            Match match = Regex.Match(key, "^[A-Za-z0-9]+");
            DConsole.Asset(match.Success,$"[RALoadMgr] GetAction 无效的匹配");
            path = m_addressableKeyMap[match.Value].ToString();
        }
        DConsole.Asset(!String.IsNullOrEmpty(path),$"[RALoadMgr] GetAction {assetReference}:{path} 无效的参数，请检查 AddressableKeyMap.json 配置");
        
        GetAction<T>(path,callback);
    }
    
    
    
    
    
    
    
    
    
}








public class RALoadItemYield : CustomYieldInstruction
{
    public override bool keepWaiting => ((RALoadItem)Current).RALoadStatus != RALoadStatus.Loaded;
    private new object Current => this;
}



public class RALoadItem : RALoadItemYield
{
    public RALoadMode RALoadMode;
    public RALoadType RALoadType;
    public RALoadStatus RALoadStatus;
    
    public string Key;
    public Sprite Sprite;
    public ResourceRequest ResourceRequestResult;
    public AsyncOperationHandle AsyncOperationHandle;
}



public enum RALoadType
{
    GameObject,
    Texture2D,
    Sprite,
    SpriteAtlas,
    TextAsset,
    Material,
}


public enum RALoadStatus
{
    NoLoad,
    Loading,
    Loaded,
}

public enum RALoadMode
{
    Resources,
    Addressable,
}