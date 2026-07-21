using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using UnityEngine.Events;
using URandom = UnityEngine.Random;
using DG.Tweening;


public static class MUtility
{
    // 贝塞尔曲线2阶  3个定位点
    public static void CalculatorBezierCurve2(Vector3 start, Vector3 center, Vector3 end, ref Vector3[] points_array)
    {
        var count = points_array.Length;
        var maxidx = count - 1;
        for (int i = 0; i <= maxidx; i++)
        {
            var t = i * 1.0f / maxidx;
            Vector3 aa = start + (center - start) * t;
            Vector3 bb = center + (end - center) * t;
            points_array[i] = aa + (bb - aa) * t;
        }
    }

    
    
    // 3D 物理投射检测
    private static RaycastHit[] m_raycast3d_hits;
    public static bool RayCastToClosed(Vector3 origin, Vector3 direction, out RaycastHit hit_closed)
    {
        m_raycast3d_hits ??= new RaycastHit[8];
        Array.Clear(m_raycast3d_hits, 0, m_raycast3d_hits.Length);
        
        int num= Physics.RaycastNonAlloc(origin, direction, m_raycast3d_hits);
        
        if (num > 0)
        {
            hit_closed = m_raycast3d_hits[0];
            for (int i = 1; i < num; i++)
            {
                var hit_item = m_raycast3d_hits[i];
                if (hit_item.distance < hit_closed.distance)
                {
                    hit_closed = hit_item;
                }
            }
        }
        else
        {
            hit_closed = default;
        }
        return num > 0;
    }



    // 3D 物理投射检测
    public static int RayCastToClosed(Vector3 origin, Vector3 direction, ref RaycastHit[] hits,bool isSort = false)
    {
        m_raycast3d_hits ??= new RaycastHit[8];
        Array.Clear(m_raycast3d_hits, 0, m_raycast3d_hits.Length);
        
        int num= Physics.RaycastNonAlloc(origin, direction, m_raycast3d_hits);
        if (isSort)
        {
            // 新实例
            hits = new RaycastHit[num];
            Array.Copy(m_raycast3d_hits,hits,num);
            var collection = hits.OrderBy((hit) => hit.distance);
            hits = collection.ToArray();
            collection = null;
        }
        else
        {
            hits = m_raycast3d_hits;
        }
        return num ;
    }
    
    
    // 3D 物理投射检测
    public static bool RayCastToClosed(Vector3 ScreenPos, out RaycastHit hit_closed,float distance = 1000,int layermask = -5,Camera camera = null)
    {
        m_raycast3d_hits ??= new RaycastHit[8];
        Array.Clear(m_raycast3d_hits, 0, m_raycast3d_hits.Length);

        if (!camera) camera = Camera.main;
        
        Ray ray = camera.ScreenPointToRay(ScreenPos);
        int num = Physics.RaycastNonAlloc(ray, m_raycast3d_hits,distance,layermask);
        if (num > 0)
        {
            hit_closed = m_raycast3d_hits[0];
            for (int i = 1; i < num; i++)
            {
                var hit_item = m_raycast3d_hits[i];
                if (hit_item.distance < hit_closed.distance)
                {
                    hit_closed = hit_item;
                }
            }
        }
        else
        {
            hit_closed = default;
        }
        return num > 0;
    }
    
        
    // 3D擦场景中 单指滑动屏幕屏幕映射到世界平面的位移增量（世界）
    // 屏幕上的两个点 转换摄像机 平面（点，法线）
    private static Plane s_plane;
    private static Vector3 s_plane_delta;

    public static Vector3 GetDeltaForPlane(Vector2 screen_start,Vector2 screen_end,Vector3 pos_world,Vector3 normal,Camera camera = null)
    {
        if(!camera) camera = Camera.main;

        Ray ray_a = camera.ScreenPointToRay(screen_start);
        Ray ray_b = camera.ScreenPointToRay(screen_end);
        s_plane.SetNormalAndPosition(normal, pos_world);

        float a_distance;
        float b_distance;
        bool a_is = s_plane.Raycast(ray_a, out a_distance);
        bool b_is = s_plane.Raycast(ray_b, out b_distance);

        s_plane_delta.Set(0,0,0);
        if (a_is && b_is)
        {
            Vector3 pos_a = ray_a.GetPoint(a_distance);
            Vector3 pos_b = ray_b.GetPoint(b_distance);
            s_plane_delta.x = pos_b.x - pos_a.x;
            s_plane_delta.y = pos_b.y - pos_a.y;
            s_plane_delta.z = pos_b.z - pos_a.z;
        }

        return s_plane_delta;
    }


    public static Vector3 GetPointForPlane(Vector2 screen,Vector3 pos_world,Vector3 normal,Camera camera = null)
    {
        if(!camera) camera = Camera.main;

        Ray ray = camera.ScreenPointToRay(screen);
        s_plane.SetNormalAndPosition(normal, pos_world);
        bool ishave = s_plane.Raycast(ray, out var a_distance);
        Vector3 point = ishave ? ray.GetPoint(a_distance) : Vector3.zero;

        return point;
    }
    
    
    
    
    
    // 屏幕分辨率适配 (x,y):目标为1 (z,w):s_screen_base为1
    private static Vector2 s_screeen_base = new Vector2(720, 1600);
    private static Vector4 s_screen_scale = Vector4.zero;
    private static bool s_screen_isfit = false;
    
    public static Vector4 GetScreenFitScale()
    {
        if (!s_screen_isfit)
        {
            s_screen_scale.x = Screen.width / s_screeen_base.x;
            s_screen_scale.y = Screen.height / s_screeen_base.y;
            s_screen_scale.z = s_screeen_base.x / Screen.width;
            s_screen_scale.w = s_screeen_base.y / Screen.height;
            s_screen_isfit = true;
        }
        return s_screen_scale;
    }




    // MonoBehabviour
    public static void DelayCallback(this MonoBehaviour self, float delay,UnityAction callback)
    {
        self.StartCoroutine(DelayCallback(delay,callback));
    }


    private static IEnumerator DelayCallback(float delay, UnityAction callback)
    {
        yield return new WaitForSecondsRealtime(delay);
        callback?.Invoke();
    }



    public static void WaitFrameEndCallback(this MonoBehaviour self,UnityAction callback)
    {
        self.StartCoroutine(FrameEndCallback(callback));
    }
    
    
    private static IEnumerator FrameEndCallback(UnityAction callback)
    {
        yield return new WaitForEndOfFrame();
        callback?.Invoke();
    }

    
    
    
    
    
    
    
    // 销毁节点下的所有子对象
    public static void DestroyChildren(Transform tr)
    {
        var count = tr.childCount;
        while (count > 0)
        {
            var index = count - 1;
            GameObject.Destroy(tr.GetChild(index).gameObject);
            count--;
        }
    }
    
    
    
    
    
    
    // ======================================== 时间相关
    public static readonly DateTime S_DateTime_Begin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    
    
    /// <summary>
    /// 是否为新的一天
    /// </summary>
    /// <returns></returns>
    public static bool IsToday(long ms)
    {
        return DateTime.Now.Date == DateTimeFromMS(ms).Date;
    }


    public static bool IsToday(DateTime dateTime)
    {
        return DateTime.Now.Date == dateTime.Date;
    }

    
    
    // 毫秒时间戳
    public static long NowMSTimestamp()
    {
        TimeSpan timeSpan = DateTime.Now - S_DateTime_Begin;
        return (long)timeSpan.TotalMilliseconds;
    }
    


    // 毫秒转时间
    public static DateTime DateTimeFromMS(long ms)
    {
        return S_DateTime_Begin.AddMilliseconds(ms);
    }



    // DataTime 转 毫秒时间戳
    public static long MSFromDataTime(DateTime dateTime)
    {
        return (long)(dateTime - S_DateTime_Begin).TotalMilliseconds;
    }



    // 格式话秒数到00:00
    public static string FormatSecond(int s)
    {
        int min = s / 60;
        int sec = s % 60;
  
        // 格式化输出  
        return $"{min:00}:{sec:00}";  
    }



    /// <summary>
    /// 字符串转DateTime 注意格式：2026-01-01 23:23:23.6
    /// </summary>
    /// <returns></returns>
    public static DateTime DateTimeFromString(string timeStr)
    {
       bool isCan = DateTime.TryParse(timeStr, out var dateTime);
       if (isCan)
       {
           return dateTime;
       }

       return S_DateTime_Begin;
    }



    /// <summary>
    /// 从旧的时间戳到现在间隔多少毫秒
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public static long TotalMsToNow(long ms)
    {
        TimeSpan timeSpan = DateTime.Now - DateTimeFromMS(ms);
        return (long)timeSpan.TotalMilliseconds;
    }
    
    
    
    
    // 时间戳字符串转long
    public static long TimestampStringToLong(string timestamp)
    {
        bool isCan = long.TryParse(timestamp, out long result);
        if (isCan)
        {
            return result;
        }

        return 0;
    }
    
    
    
    
    
    // =========================================== UI界面
    public static void PopUpAni(RectTransform rectTransform)
    {
        rectTransform.localScale = Vector3.one * 0.3f;
        rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
}




