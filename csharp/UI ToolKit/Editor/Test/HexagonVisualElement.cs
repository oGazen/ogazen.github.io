using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class HexagonVisualElement : VisualElement
{
    /* 工厂类 */
    public new class UxmlFactory : UxmlFactory<HexagonVisualElement, UxmlTraits> { }

    /* xml 描述类 */
    public class UxmlVector2AttributeDescription : TypedUxmlAttributeDescription<Vector2>
    {
        public UxmlVector2AttributeDescription() { }

        public override string defaultValueAsString { get; }

        public override Vector2 GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
        {
            string shadowOffsetStr;
            bool isHave = bag.TryGetAttributeValue("shadow-offset", out shadowOffsetStr);
            if (isHave)
            {
                shadowOffsetStr = shadowOffsetStr.Trim();
                Match match = Regex.Match(shadowOffsetStr, @"^\((.*)\)$");
                string match_target = Regex.Replace(match.Groups[1].Value, " *", "");
                string[] arr = match_target.Split(",");
                return new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
            }
            return Vector2.zero;
        }
    }


    /* 特性类 */
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlColorAttributeDescription m_mainColor = new UxmlColorAttributeDescription { name = "main-color" };

        UxmlColorAttributeDescription m_shadowColor = new UxmlColorAttributeDescription { name = "shadow-color" };
        UxmlVector2AttributeDescription m_shadowOffset = new UxmlVector2AttributeDescription { name = "shadow-offset" };

        UxmlColorAttributeDescription m_outlineColor = new UxmlColorAttributeDescription { name = "outline-color" };
        UxmlFloatAttributeDescription m_outlineWidth = new UxmlFloatAttributeDescription { name = "outline-width" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var customelem = (HexagonVisualElement)ve;
            customelem.mainColor = m_mainColor.GetValueFromBag(bag, cc);

            customelem.shadowColor = m_shadowColor.GetValueFromBag(bag, cc);
            customelem.shadowOffset = m_shadowOffset.GetValueFromBag(bag, cc);

            customelem.outlineColor = m_outlineColor.GetValueFromBag(bag, cc);
            customelem.outlineWidth = m_outlineWidth.GetValueFromBag(bag, cc);
        }
    }


    /* 参数类 */
    private static Color DefultShadowColor = new Color(0, 0, 0, 0.5f);
    private static Color DefultOutlineColor = new Color(0, 0, 0, 0.5f);

    /* 子类参数 */
    public class ParamVariable
    {
        public readonly float width;
        public readonly float heigth;

        public readonly int mainIndex;
        public readonly Color mainColor;

        public Color shadowColor;
        public Color outlineColor;
        public Vector2 shadowOffset;
        public float outlineWidth;

        public ParamVariable(float _w, float _h, Color _c, int index)
        {
            this.mainIndex = index;
            this.width = _w;
            this.heigth = _h;
            this.mainColor = _c;

            shadowColor = DefultShadowColor;
            shadowOffset = new Vector2(1.5f, 1.8f);
            outlineColor = DefultOutlineColor;
            outlineWidth = 0.5f;
        }
    }


    /* 自定义变量 */
    public Color mainColor { get; private set; }
    public Color shadowColor { get; set; }
    public Vector2 shadowOffset { get; set; }
    public Color outlineColor { get; set; }
    public float outlineWidth { get; set; }

    /* 用户数据 */
    public readonly bool ishaveUserdata = false;
    public readonly Vector2Int position;
    public readonly int type_index = 0;
    private int type_num;
    public int type_Num
    {
        get { return this.type_num; }
        set { if (value <= 1) this.type_num = 1; else this.type_num = value; }
    }

    /* 构造 */
    public HexagonVisualElement(int index, int num = 1)
    {
        this.ishaveUserdata = true;
        this.type_index = index;
        this.type_Num = num;
        this.AddDrawFunc();
    }

    public HexagonVisualElement()
    {
        this.AddDrawFunc();
    }

    public HexagonVisualElement(Vector2Int vector2Int)
    {
        this.position = vector2Int;
        this.AddDrawFunc();
    }

    private void AddDrawFunc()
    {
        generateVisualContent += OnGenerateVisualContent_Shadow;
        generateVisualContent += OnGenerateVisualContent_Outline;
        generateVisualContent += OnGenerateVisualContent;
    }

    public void Init(ParamVariable paramVariable)
    {
        this.style.width = paramVariable.width;
        this.style.height = paramVariable.heigth;
        this.mainColor = paramVariable.mainColor;

        this.shadowColor = paramVariable.shadowColor;
        this.shadowOffset = paramVariable.shadowOffset;

        this.outlineColor = paramVariable.outlineColor;
        this.outlineWidth = paramVariable.outlineWidth;
    }

    private void OnGenerateVisualContent(MeshGenerationContext obj)
    {
        var vertices = new Vertex[7];
        var indices = new ushort[] { 0, 1, 6, 6, 1, 2, 2, 3, 6, 6, 3, 4, 4, 5, 6, 6, 5, 0 };
        var mesh = obj.Allocate(7, 18);

        // 顶点位置
        {
            var style = this.localBound;
            var size_width = style.width;
            var size_height = style.height;
            var minheigth = Mathf.Sqrt(Mathf.Pow(size_width, 2) - Mathf.Pow(size_width / 2, 2));

            var center_x = size_width / 2;
            var center_y = size_height / 2;

            vertices[0].position = new Vector3(center_x - size_width / 4, center_y + minheigth / 2, Vertex.nearZ);
            vertices[1].position = new Vector3(0, center_y, Vertex.nearZ);
            vertices[2].position = new Vector3(center_x - size_width / 4, center_y - minheigth / 2, Vertex.nearZ);
            vertices[3].position = new Vector3(center_x + size_width / 4, center_y - minheigth / 2, Vertex.nearZ);
            vertices[4].position = new Vector3(size_width, center_y, Vertex.nearZ);
            vertices[5].position = new Vector3(center_x + size_width / 4, center_y + minheigth / 2, Vertex.nearZ);
            vertices[6].position = new Vector3(center_x, center_y, Vertex.nearZ);
        }

        // 顶点颜色
        for (int i = 0; i < vertices.Length; i++)
            vertices[i].tint = mainColor;


        // 赋值
        mesh.SetAllVertices(vertices);
        mesh.SetAllIndices(indices);
    }


    private void OnGenerateVisualContent_Shadow(MeshGenerationContext obj)
    {
        if (this.shadowOffset.x <= 0 && this.shadowOffset.y <= 0) return;

        var vertices = new Vertex[7];
        var indices = new ushort[] { 0, 1, 6, 6, 1, 2, 2, 3, 6, 6, 3, 4, 4, 5, 6, 6, 5, 0 };
        var mesh = obj.Allocate(7, 18);

        {
            var style = this.localBound;
            var size_width = style.width;
            var size_height = style.height;
            var minheigth = Mathf.Sqrt(Mathf.Pow(size_width, 2) - Mathf.Pow(size_width / 2, 2));

            var center_x = size_width / 2;
            var center_y = size_height / 2;
            vertices[0].position = new Vector3(center_x - size_width / 4, center_y + minheigth / 2, Vertex.nearZ);
            vertices[1].position = new Vector3(0, center_y, Vertex.nearZ);
            vertices[2].position = new Vector3(center_x - size_width / 4, center_y - minheigth / 2, Vertex.nearZ);
            vertices[3].position = new Vector3(center_x + size_width / 4, center_y - minheigth / 2, Vertex.nearZ);
            vertices[4].position = new Vector3(size_width, center_y, Vertex.nearZ);
            vertices[5].position = new Vector3(center_x + size_width / 4, center_y + minheigth / 2, Vertex.nearZ);
            vertices[6].position = new Vector3(center_x, center_y, Vertex.nearZ);

            Vector3 vector3 = new Vector3(shadowOffset.x, shadowOffset.y, 0);
            for (int i = 0; i < vertices.Length; i++)
            { vertices[i].position += vector3; vertices[i].tint = shadowColor; }
        }


        mesh.SetAllVertices(vertices);
        mesh.SetAllIndices(indices);
    }


    private void OnGenerateVisualContent_Outline(MeshGenerationContext obj)
    {
        if (this.outlineWidth <= 0) return;

        var vertices = new Vertex[7];
        var indices = new ushort[] { 0, 1, 6, 6, 1, 2, 2, 3, 6, 6, 3, 4, 4, 5, 6, 6, 5, 0 };
        var mesh = obj.Allocate(7, 18);

        // 顶点位置
        {
            var style = this.localBound;
            var size_width = style.width;
            var size_height = style.height;
            var minheigth = Mathf.Sqrt(Mathf.Pow(size_width, 2) - Mathf.Pow(size_width / 2, 2));

            var center_x = size_width / 2;
            var center_y = size_height / 2;
            var outlineX = outlineWidth / 2;
            var outlineY = outlineWidth * Mathf.Sin(60 * Mathf.PI / 180);

            vertices[0].position = new Vector3(center_x - size_width / 4 - outlineX, center_y + minheigth / 2 + outlineY, Vertex.nearZ);
            vertices[1].position = new Vector3(0 - outlineWidth, center_y, Vertex.nearZ);
            vertices[2].position = new Vector3(center_x - size_width / 4 - outlineX, center_y - minheigth / 2 - outlineY, Vertex.nearZ);
            vertices[3].position = new Vector3(center_x + size_width / 4 + outlineX, center_y - minheigth / 2 - outlineY, Vertex.nearZ);
            vertices[4].position = new Vector3(size_width + outlineWidth, center_y, Vertex.nearZ);
            vertices[5].position = new Vector3(center_x + size_width / 4 + outlineX, center_y + minheigth / 2 + outlineY, Vertex.nearZ);
            vertices[6].position = new Vector3(center_x, center_y, Vertex.nearZ);
        }

        // 顶点颜色
        for (int i = 0; i < vertices.Length; i++)
            vertices[i].tint = outlineColor;


        // 赋值
        mesh.SetAllVertices(vertices);
        mesh.SetAllIndices(indices);
    }




}
