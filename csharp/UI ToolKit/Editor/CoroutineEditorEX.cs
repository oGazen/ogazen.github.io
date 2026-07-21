using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using System.Text.RegularExpressions;


/// <summary>
/// 自定义编辑器协程，可以协程嵌套(自测可用)
/// 适用：派生于CustomYieldInstruction
///     一帧
///     WaitForSecondsRealtime
///     WaitUntil
///     WaitWhile
///     ......
/// </summary>
public class CoroutineEditorEX
{
    private static List<IEnumerator> enumerators_list; // 编辑器协程列表
    private static Queue<int> queue_removeList; // 清除列表索引
    private static Dictionary<int, Stack<IEnumerator>> enumerator_list_item; //用于保存协程列表中的协程嵌套
    static CoroutineEditorEX()
    {
        enumerators_list = new List<IEnumerator>();
        queue_removeList = new Queue<int>();
        enumerator_list_item = new Dictionary<int, Stack<IEnumerator>>();
    }


    public static void Start(IEnumerator enumerator)
    {
        enumerators_list.Add(enumerator);
        Debug.Log($"[CoroutineEditorEX][UpdateCheck] {enumerator.GetType().Name} [id:{enumerator.GetHashCode()}] 当前协程开始！！！");
        if (enumerators_list.Count <= 1)
            EditorApplication.update += UpdateCheck;
    }

    private static void UpdateCheck()
    {
        if (enumerators_list.Count > 0)
        {
            for (int i = 0; i < enumerators_list.Count; i++)
            {
                var item = enumerators_list[i];
                CheckWait(item, i);
            }
            while (queue_removeList.Count > 0)
            {
                var index = queue_removeList.Dequeue();
                if (enumerators_list.Count > index)
                {
                    Debug.Log($"[CoroutineEditorEX][UpdateCheck] {enumerators_list[index].GetType().Name} [id:{enumerators_list[index].GetHashCode()}] 当前协程结束！！！");
                    enumerators_list.RemoveAt(index);
                }
            }
        }
        else
        {
            EditorApplication.update -= UpdateCheck;
            Debug.Log($"[CoroutineEditorEX][UpdateCheck] 全部编辑器协程已结束！！！");
        }
    }


    // 检查不同类型的等待
    private static void CheckWait(IEnumerator item, int index)
    {
        if (CheckWait_Item_Stack(index)) // 深层栈检测
            return;

        // 等待一帧[第一帧]
        bool isCustomYield = item.Current is CustomYieldInstruction;
        //Debug.Log($"isCustomYield:{isCustomYield}");
        if (!isCustomYield)
        {
            bool isnodel = item.MoveNext();
            if (!isnodel)
                queue_removeList.Enqueue(index);
            else
                IEnumerator_Item_Stack(item, index, true);
        }
        // 等待毫秒数; [自定义yield效果] :CustomYieldInstruction
        else
        {
            var second = item.Current as CustomYieldInstruction;
            if (!second.keepWaiting)
            {
                bool isnodel = item.MoveNext();
                if (!isnodel)
                    queue_removeList.Enqueue(index);
                else
                    IEnumerator_Item_Stack(item, index, true);
            }
        } // 表层检测


    }



    // 对于嵌套协程协程进行更新处理，返回当前协程是否还有存在嵌套
    private static bool CheckWait_Item_Stack(int index)
    {
        bool isHave = enumerator_list_item.ContainsKey(index);
        if (isHave)
        {
            var item = enumerator_list_item[index];
            IEnumerator cur = item.Peek();
            bool isCustomYield = cur.Current is CustomYieldInstruction;

            if (!isCustomYield)
            {
                bool isnodel = cur.MoveNext();
                if (!isnodel)
                    CheckWait_Item_Stack_Todo(index);
                else
                    IEnumerator_Item_Stack(cur, index, true);
            }
            else
            {
                var second = cur.Current as CustomYieldInstruction;
                if (!second.keepWaiting)
                {
                    //检查下一次movenext是否还是可迭代操作，如果时继续压入栈中更新检测
                    bool isnodel = cur.MoveNext();
                    if (!isnodel)
                        CheckWait_Item_Stack_Todo(index);
                    else
                        IEnumerator_Item_Stack(cur, index, true);
                }
            }

            if (item.Count <= 0)
            {
                enumerator_list_item.Remove(index);
                isHave = false;
            }
        }

        return isHave;
    }

    // 检查是都需要出栈
    private static void CheckWait_Item_Stack_Todo(int index)
    {
        enumerator_list_item[index].Pop();
        IEnumerator next;
        var isContains = enumerator_list_item[index].TryPeek(out next);
        if (isContains)
            IEnumerator_Item_Stack(next, index);
    }


    // 对协程嵌套递归处理 发现规律[IEnumerator]每次第一此current都为空，所以每次从第二次current开始检测
    private const string pattern = @"^<(\S+)>";
    private static void IEnumerator_Item_Stack(IEnumerator enumerator, int index, bool isJump = false)
    {
        if (!isJump) //是否特殊类型，跳过movenext迭代
        {
            bool isHav = enumerator.MoveNext();
            if (!isHav) return;
        }
        if (enumerator.Current == null) return; // 判空检测临时处理

        var name_input = enumerator.Current.GetType().Name;
        Match match = Regex.Match(name_input, pattern);
        // Debug.Log($"item.Current is IEnumerator:{match.Success} :{match.Value}");
        if (match.Success)
        {
            if (!enumerator_list_item.ContainsKey(index))
                enumerator_list_item.Add(index, new Stack<IEnumerator>());
            enumerator_list_item[index].Push(enumerator.Current as IEnumerator);
            IEnumerator_Item_Stack(enumerator.Current as IEnumerator, index);
        }


    }

}
