using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using WeChatWASM;

using UDebug = UnityEngine.Debug;
using URandom = UnityEngine.Random;
using UObject = UnityEngine.Object;
using System.Runtime.InteropServices;
using ArrowHeadBoxes;

namespace Own
{

    [Preserve]
    public class WebBridgeManager
    {
        public static WebBridgeManager Instance = new WebBridgeManager();
        private WebBridgeManager()
        {
            UDebug.Log($"WebBridgeManager structure -> ");
            GameObject obj = new GameObject();
            UObject.DontDestroyOnLoad(obj);
            obj.name = "OwnBridgeJS";
            obj.AddComponent<WebBridge>();
        }


        // 初始化
        public void Init()
        {
            UDebug.Log($"wgz+++ Init -> ");
        }


        #region

        [DllImport("__Internal")]
        private static extern void Example(string successKey, string failKey);


        [DllImport("__Internal")]
        private static extern string GetVersion();


        [DllImport("__Internal")]
        private static extern bool IsDevDebug();


        [DllImport("__Internal")]
        private static extern bool IsJumpAd();


        [DllImport("__Internal")]
        private static extern bool SubcribeWx(string successKey, string failKey);

        #endregion



        // 例子
        public void Example(ExampleParam param)
        {

#if !UNITY_EDITOR && UNITY_WEBGL
            Example(
                CallbackHandler.Add(param.success),
                CallbackHandler.Add(param.fail)
                );
#endif


        }



        // 版本号字符串
        private string m_version;
        public string Version()
        {

#if !UNITY_EDITOR && UNITY_WEBGL
            if (m_version == null)
            {
                m_version = GetVersion();
            }

            return m_version;
#elif UNITY_EDITOR
            return "UnityEditor";
#else
            return "Other";
#endif


        }




        // 是否直接跳过广告
        public bool IsSkipAd()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            return IsJumpAd();
#elif UNITY_EDITOR
            return true;
#else
            return true;
#endif
        }



        // 是否开发调试模式
        public bool IsDevGMDebug()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            return IsDevDebug();
#elif UNITY_EDITOR
            return true;
#else
            return true;
#endif
        }



        // 用户订阅 仅游戏更新通知
        public void SubcribeWxOnly_WHATS_NEW(SubcribeParam subcribeParam)
        {

#if !UNITY_EDITOR && UNITY_WEBGL
            SubcribeWx(
                CallbackHandler.Add(subcribeParam.success),
                CallbackHandler.Add(subcribeParam.fail)
                );
#elif UNITY_EDITOR
            UDebug.Log("订阅游戏更新通知仅在微信平台可用");
#else
            return "Other";
#endif
        }




    }






    [Preserve]
    public class WebBridge : MonoBehaviour
    {
        private static WebBridge webBridgeInstance;
        public static WebBridge Ins => webBridgeInstance;


        private void Awake()
        {
            if (webBridgeInstance == null) webBridgeInstance = this;
            else Destroy(this);
        }





        // 管控JS 回调 Unity
        public void HandleJsCallBack(string message)
        {
            UDebug.Log($"[WebBridge] HandleJsCallBack -> message:{message}");
            CallbackHandler.InvokeResponseCallback<BaseResponse>(message);
        }



        #region 测试

        public void TestOwn()
        {
            UDebug.Log($"[WebBridge] TestOwn ->");

        }

        #endregion

    }




    [Preserve]
    public class CallbackHandler
    {
        private static readonly Hashtable responseHT = new Hashtable();

        private static int htCounter = 0;

        private static int GenarateCallbackId()
        {
            if (htCounter > 1000000)
            {
                htCounter = 0;
            }

            htCounter++;
            return htCounter;
        }

        public static string Add<T>(Action<T> callback) where T : BaseResponse
        {
            if (callback == null)
            {
                return "";
            }
            var key = MakeKey();
            responseHT.Add(key, callback);
            return key;
        }

        public static string MakeKey()
        {
            int id = GenarateCallbackId();
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timestamp = Convert.ToInt64(ts.TotalSeconds);
            var key = timestamp.ToString() + '-' + id;
            return key;
        }

        public static void InvokeResponseCallback<T>(string str) where T : BaseResponse
        {
            if (!string.IsNullOrEmpty(str))
            {
                T res = JsonUtility.FromJson<T>(str);
                var id = res.callbackId;

                Callback(id, res);
            }
        }

        public static void Callback<T>(string id, T res)
        {
            if (responseHT.ContainsKey(id))
            {
                var callback = (Action<T>)responseHT[id];
                callback(res);
                responseHT.Remove(id);
            }
            else
            {
                UDebug.LogError($"callback id not found, id: {id}");
            }
        }

    }

    [Preserve]
    public class BaseResponse
    {
        public string callbackId; // 回调id,调用者不需要关注
        public string resultStr; // 返回的结果字符串信息，具体含义视具体情况
    }

    [Preserve]
    public class BaseActionParam<T>
    {
        public System.Action<T> success; //接口调用成功的回调函数
        public System.Action<T> fail; //接口调用失败的回调函数	
    }

    [Preserve]
    public class ExampleParam : BaseActionParam<BaseResponse>
    {
        public string xxx;
    }

    [Preserve]
    public class SubcribeParam : BaseActionParam<BaseResponse>
    {
        // 游戏更新订阅
        // public string msgType = "SYS_MSG_TYPE_WHATS_NEW";
    }
}




