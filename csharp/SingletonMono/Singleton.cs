using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

public class Singleton<T> where T : class
{
    private static T _ins;
    public static T Ins
    {
        get
        {
            if (_ins == null)
            {
                _ins = ExClass.CreateClassIns<T>();
            }
            return _ins;
        }
    }
}

public static class ExClass
{
    public static T CreateClassIns<T>()
    {
        var type = typeof(T);
        try
        {
            return (T)type.Assembly.CreateInstance(type.FullName, true, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(string.Format("{0}(单例模式下，构造函数必须为private)", ex.Message));
        }
    }

}
