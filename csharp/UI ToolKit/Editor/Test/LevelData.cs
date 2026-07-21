using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyData;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

public class LevelData : ScriptableObject
{
    /* 加载的所有关卡列表 */
    private ItemData m_curdata; // 当前位置Item数据

    [DisableField, SerializeField]
    private Vector2Int m_vec2; // 坐标

    [SerializeField]
    private GroundType m_base_type; // 底Box类型

    [SerializeField, Min(1)]
    private int m_base_value = 1; // 底类型对应值

    [SerializeField, Min(1)]
    private int m_base_target = 1; // Level目标
    public int target { get { return m_base_target; } }

    [SerializeField]
    private SpecialGroundType m_type; // 上层Box类型

    [DisableField, SerializeField]
    private string m_values; // 上层六边形数组字符串数据

    [SerializeField]
    private Itemlist[] m_values_dic; // 上层六边形字典数据


    [System.Serializable]
    private class Itemlist
    {
        public readonly HexagonVisualElement element; // 当个六边形元素

        [DisableField, SerializeField]
        private Color color; // 六边形颜色

        [DisableField, SerializeField]
        private MyExtend.ColorEnum colorEnum; // 六边形颜色枚举

        [Range(1, 9)]
        public int num = 1; // 六边形元素数量

        public Itemlist(HexagonVisualElement elem)
        {
            this.element = elem;
            this.color = MyExtend.ColorArray[elem.type_index];
            this.num = elem.type_Num;
            this.colorEnum = (MyExtend.ColorEnum)elem.type_index;
        }

    }

    [CustomPropertyDrawer(typeof(Itemlist))]
    private class ItemlistDrawer : PropertyDrawer
    {
        private float baseHeight = EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = label.text.Split(',');

            Rect rect = new Rect(position);
            rect.height = baseHeight;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("color"), EditorGUIUtility.TrTextContent(names[0]));

            Rect rect2 = new Rect(rect);
            rect2.y += baseHeight;
            EditorGUI.PropertyField(rect2, property.FindPropertyRelative("colorEnum"), EditorGUIUtility.TrTextContent(names[1]));

            Rect rect3 = new Rect(rect2);
            rect3.y += baseHeight;
            EditorGUI.PropertyField(rect3, property.FindPropertyRelative("num"), EditorGUIUtility.TrTextContent(names[2]));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        { return baseHeight * 3; }
    }


    public void SetItemData(ItemData data)
    {
        this.m_curdata = data;
        // 基本
        this.m_base_type = data.ground.type;
        this.m_base_value = data.ground.value;
        this.m_vec2 = new Vector2Int(data.ground.x, data.ground.y);
        // 特殊
        this.m_type = data.specialGround.propType;


        /* 特殊六方格摆放队列 */
        if (data.isHaveElemList)
        {
            var list = data.elements;

            // 字典
            this.m_values_dic = new Itemlist[list.Count];
            for (int i = 0; i < list.Count; i++)
            { var item = list[i]; m_values_dic[i] = new Itemlist(item); }

            // 值字符串
            var specialdata = data.specialGround;
            specialdata.values_out.Clear();
            var temp = m_values_dic.Select((item) =>
            {
                var len = item.num;
                var arr = new int[len];
                for (int i = 0; i < len; i++)
                    arr[i] = item.element.type_index + 1;
                return arr;
            });
            foreach (var item in temp)
                specialdata.values_out.AddRange(item);

            // 字符串
            this.m_values = string.Join(',', specialdata.values_out);
        }
        else
        { this.m_values_dic = null; this.m_values = null; }
    }

    private void OnValidate()
    {
        if (m_curdata == null) return;

        m_curdata.ground.type = m_base_type;
        m_curdata.ground.value = m_base_value;
        m_curdata.specialGround.propType = m_type;

        /* 更新队列值 */
        if (m_values_dic == null) return;
        var specialdata = m_curdata.specialGround;
        var data = this.m_curdata;
        var valuedic = this.m_values_dic;

        // 仅num更新
        specialdata.values_out.Clear();
        var temp = valuedic.Select((item) =>
        {
            var len = item.num;
            item.element.type_Num = len;

            var arr = new int[len];
            for (int i = 0; i < len; i++)
                arr[i] = item.element.type_index + 1;
            return arr;
        });
        foreach (var item in temp)
            specialdata.values_out.AddRange(item);

        // 字符串
        this.m_values = string.Join(',', specialdata.values_out);
    }

}
