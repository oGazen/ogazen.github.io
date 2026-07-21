using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

namespace MyData
{
    public enum GroundType
    {
        Normal = 1, // 正常地面
        AdVideo = 2, // Video广告解锁
        Target = 3, // 目标解锁
        Board = 4, // 木板Lock解锁
    }

    public enum SpecialGroundType
    {
        Normal = 1, // 正常
        Frozen = 2, // 冰冻
    }


    [Serializable]
    public class Ground
    {
        public int x;
        public int y;
        public GroundType type = GroundType.Normal;
        public int value = 1;
    }


    [Serializable]
    public class SpecialGround
    {
        public int x;
        public int y;

        [NonSerialized]
        public readonly List<int> values_out;
        public SpecialGroundType propType = SpecialGroundType.Normal;

        [SerializeField]
        private int[] values;

        public SpecialGround()
        { values_out = new List<int>(); }

        public void Check()
        {
            if (values_out.Count <= 0) values = new int[0];
            else values = values_out.ToArray();
        }

    }


    [Serializable]
    public class Level
    {
        public int target = 1;

        [NonSerialized]
        public readonly List<Ground> grounds_out;
        [NonSerialized]
        public readonly List<SpecialGround> inits_out;

        [SerializeField]
        private Ground[] grounds;
        [SerializeField]
        private SpecialGround[] inits;

        public Level()
        { grounds_out = new List<Ground>(); inits_out = new List<SpecialGround>(); }

        public void Check()
        {
            if (grounds_out.Count <= 0) grounds = new Ground[0];
            else grounds = grounds_out.ToArray();

            if (inits_out.Count <= 0) inits = new SpecialGround[0];
            else inits = inits_out.ToArray();

            for (int i = 0; i < inits.Length; i++)
            { var item = inits[i]; item.Check(); }
        }


    }


    public class ItemData
    {
        /* cache data */
        public readonly Vector2Int posv2 = new Vector2Int(-1, -1); // <column,row>
        public readonly HexagonVisualElement elementBase;

        public ItemData(Vector2Int vector2, HexagonVisualElement element)
        {
            this.posv2 = vector2;
            this.elementBase = element;
        }

        // 六边形块数据
        public HexagonVisualElement elementGround;
        public readonly List<HexagonVisualElement> elements = new List<HexagonVisualElement>();
        public bool isHaveElemList { get { return elements.Count > 0; } }
        public bool isHaveElemGround { get { return elementGround != null; } }

        /* json data */
        public readonly Ground ground = new Ground();
        public readonly SpecialGround specialGround = new SpecialGround();

        /* function */
        public void Clear()
        {
            elements.Clear();
            elementGround = null;

            ground.x = 0;
            ground.y = 0;
            ground.type = GroundType.Normal;
            ground.value = 1;

            specialGround.x = 0;
            specialGround.y = 0;
            specialGround.values_out.Clear();
            specialGround.propType = SpecialGroundType.Normal;
        }

    }


    public static class MyExtend
    {
        /* JSON Convert */
        public static string ToJson(this Level level)
        { var str = JsonUtility.ToJson(level, true); return str; }

        public static Level ToLevelData(this Level baselevel, string str)
        { Level level = JsonUtility.FromJson<Level>(str); return level; }

        /* 颜色列表 */
        public static readonly Color[] ColorArray = new Color[5]
        {
            Color.white,
            Color.green,
            Color.red,
            Color.yellow,
            Color.cyan,
        };

        /* 颜色列表枚举 */
        public enum ColorEnum
        {
            white,
            green,
            red,
            yellow,
            cyan,
        }

    }



}



