using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text;
using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

using UDebug = UnityEngine.Debug;

public static class GeneratedTest
{
    [MenuItem("GameObject/GeneratedTest", true)]
    public static bool GeneratedValidateFunc()
    {
        var obj = Selection.activeTransform;
        var ishave = CheckSiblingSameName(obj);
        if (ishave) return false;

        var comps = obj.GetComponents<Component>();
        for (int i = 0; i < comps.Length; i++)
        {
            var comp = comps[i];
            var attributes = comp.GetType().CustomAttributes;
            foreach (var item in attributes)
            {
                if (item.AttributeType == typeof(CheckGeneratedAttribute))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MenuItem("GameObject/GeneratedTest")]
    private static void GenIntelliSense()
    {
        var obj = Selection.activeTransform;
        var comps = obj.GetComponents<Component>();
        for (int i = 0; i < comps.Length; i++)
        {
            var comp = comps[i];
            var attributes = comp.GetType().CustomAttributes;
            foreach (var item in attributes)
            {
                if (item.AttributeType == typeof(CheckGeneratedAttribute))
                {
                    var annotation = GetHeaderAnnotation();
                    if (string.IsNullOrEmpty(annotation))
                    {
                        UDebug.Log("生成失败请检查");
                        return;
                    }

                    var result = GenNewScript(comp);
                    var classname = comp.GetType().Name;
                    var debuglog = result.ToString();

                    /* 正式生成 */

                    var newcs = string.Format(template, classname, debuglog, annotation);
                    var path = Application.dataPath + "/Script/Gen";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    using (var stream = File.CreateText(path + "/autogen_" + classname + ".cs"))
                    {
                        stream.Write(newcs);
                        stream.Close();
                        stream.Dispose();
                    }
                    AssetDatabase.Refresh();

                }
            }
        }

    }




    #region 模板字符串

    // 部分类
    public const string template = @"{2}
using UnityEngine;
using UnityEngine.UI;

public partial class {0}
{{
    {1}
}}
";

    // 字段变量
    public const string proptemplate = @"
    [SerializeField,CheckGeneratedVisibly(""m_IsShowGenDetail"")] private {0} {1};
";

    // 变量名
    public const string vartempalate = "gen{0}_{1}";


    public const string hearderannotation = @"
///
/// ©zhuoyou
/// auto-generated
/// auth : {0}
/// createtime : {1}
///
";

    #endregion


    private static StringBuilder GenNewScript(Component comp, StringBuilder stringBuilder = null, string prev = null)
    {
        var self = comp.transform;
        var comps = self.GetComponents<Component>();

        string selfname = null;
        if (stringBuilder != null)
        {
            selfname = string.Format("{0}_{1}", prev, self.gameObject.name);
        }


        /* 本对象 */
        if (stringBuilder == null)
        { stringBuilder = new StringBuilder(); }
        foreach (var item in comps)
        {
            // 仅对本对象处理
            var type = item.GetType();
            if (string.IsNullOrEmpty(selfname) && type == comp.GetType()) continue;
            if (type == typeof(Transform)) continue;

            // 匹配非字母字符串
            var name = item.GetType().Name;
            var pathname = selfname;
            if (!string.IsNullOrEmpty(selfname))
                pathname = Regex.Replace(selfname, @"\W+", "");

            var variablename = string.Format(vartempalate, pathname, name.ToLower());
            var strline = string.Format(proptemplate, name, variablename);
            stringBuilder.Append(strline);
        }


        /* 孩子对象 */
        var len = self.childCount;
        for (int i = 0; i < len; i++)
        {
            var childtr = self.GetChild(i);
            GenNewScript(childtr, stringBuilder, selfname);
        }
        return stringBuilder;
    }




    /* 返回值：组件类型，变量名，子节点字符串 */
    public static List<Tuple<Type, string, string>> GetComps(Component comp, List<Tuple<Type, string, string>> tuples = null, string findname = null)
    {
        var comps = comp.transform.GetComponents<Component>();

        var index = 0;
        string basename = null;
        if (tuples != null) basename = string.Format("{0}_{1}", findname, comp.gameObject.name);

        if (tuples == null)
        { tuples = new List<Tuple<Type, string, string>>(); }
        foreach (var item in comps)
        {
            var type = item.GetType();
            if (string.IsNullOrEmpty(basename) && type == comp.GetType()) continue;
            if (type == typeof(Transform)) continue;

            var name = item.GetType().Name;
            var pathname = basename;
            if (!string.IsNullOrEmpty(basename))
                pathname = Regex.Replace(basename, @"\W+", "");

            var variablename = string.Format(vartempalate, pathname, name.ToLower());
            var findpath = string.IsNullOrEmpty(basename) ? null : basename.Replace("_", "/").Substring(1);
            var tuple = new Tuple<Type, string, string>(type, variablename, findpath);
            tuples.Add(tuple);

            index++;
        }


        var len = comp.transform.childCount;
        for (int i = 0; i < len; i++)
        {
            var child = comp.transform.GetChild(i);
            GetComps(child, tuples, basename);
        }
        return tuples;
    }



    /* 同1层级是否相同名字对象 */
    private static bool CheckSiblingSameName(Component comp)
    {
        bool ishave = false;
        var tr = comp.transform;
        List<string> names = new List<string>();

        for (int i = 0; i < tr.childCount; i++)
        {
            var childtr = tr.GetChild(i);
            if (childtr.childCount > 0)
            { CheckSiblingSameName(childtr); }


            if (names.Contains(childtr.gameObject.name))
            {
                ishave = true;
                UDebug.LogError("[GeneratedTest]操作禁用，子节点存在相同名字的对象，请检查");
                return ishave;
            }
            else names.Add(childtr.gameObject.name);
        }

        return ishave;
    }


    private static string GetHeaderAnnotation()
    {
        var regkey = Registry.LocalMachine;
        if (regkey == null) return null;

        var regsubkey = regkey.OpenSubKey(@"SOFTWARE\TortoiseSVN");
        if (regsubkey == null)
        {
            UDebug.Log("未安装[TortoiseSVN]客户端");
            return null;
        }

        var installpath = regsubkey.GetValue("Directory").ToString();
        var exepath = Path.Combine(installpath, @"bin\svn.exe");
        if (!File.Exists(exepath))
        {
            UDebug.Log("未安装[TortoiseSVN CMD]");
            return null;
        }

        string authname = null;
        using (var process = new Process())
        {
            process.StartInfo.Arguments = "auth";
            process.StartInfo.FileName = exepath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            //proc.StartInfo.Verb = "runas" // 管理员模式

            process.Start();
            process.StandardInput.AutoFlush = true;
            process.StandardInput.WriteLine("exit");

            string output = process.StandardOutput.ReadLine();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!Regex.IsMatch(output, "Username", RegexOptions.IgnoreCase))
            {
                output = process.StandardOutput.ReadLine();
                if (output == null)
                {
                    var seconds = stopwatch.ElapsedMilliseconds / 1000f;
                    UDebug.Log($"未找到[Svn Username] [{seconds}]");
                    stopwatch.Stop();
                    output = "Username:null";
                    break;
                }
            }
            process.WaitForExit();
            process.Close();

            var linestrarr = output.Split(':');
            authname = linestrarr[1].Trim();
        }

        var str = string.Format(hearderannotation, authname, DateTime.Now.ToString());
        return str;
    }



    /* 编译结束回调 */
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        //EditorUtility.InstanceIDToObject();

    }




}

