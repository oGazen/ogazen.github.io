using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Interactions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using MyData;

public class DragAndDropWindows : EditorWindow
{
    [MenuItem("Window/UI Toolkit/DragAndDropWindows")]
    public static void ShowExample()
    {
        DragAndDropWindows wnd = GetWindow<DragAndDropWindows>();
        wnd.titleContent = new GUIContent("DragAndDropWindows");
    }

    // 六边形特殊百分比
    private float[] m_percent_hexagon = new float[4]
    {
        0,
        1f/4,
        2f/4,
        3f/4,
    };

    // 两个列表
    private ListView m_hexagonlist;
    private ListView m_cfglist;
    private ScrollView m_scrollcontent;
    private ScrollView m_scrolldetailinfo;

    // 临时变量
    private GroupBox temp_middle;
    private GroupBox temp_detail;
    private GroupBox temp_btnsgroup;

    // 拖放脚本
    private DragAndDropManipulator m_dragAndDropManipulator;

    // ScrollContent 注册事件
    private bool m_scrollcontent_iscan = false;
    private float m_scrollconten_speedrate = 1f; // (0,1]

    // 用户数据
    private Level m_current_level;
    private LevelData m_userdata;
    private ItemData[,] m_totalItemdata;
    private ItemData[][] m_totalItemdata_oblique;

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Test/DragAndDropWindows.uxml");
        //VisualElement labelFromUXML = visualTree.Instantiate();
        //root.Add(labelFromUXML);
        visualTree.CloneTree(root);

        // 内容展示取
        ScrollView scrollView = root.Q<ScrollView>("scrollcontent");
        this.m_scrollcontent = scrollView;
        InitScrollContent(scrollView);

        this.m_dragAndDropManipulator = new DragAndDropManipulator(scrollView, root, OnDropVisualElem, OnDragVisualElem);

        // 六边形列表
        ListView listView = root.Q<ListView>("hexagon");
        this.m_hexagonlist = listView;
        InitHexagonList(listView);

        // 配置列表
        ListView cfglistview = root.Q<ListView>("config");
        this.m_cfglist = cfglistview;
        InitConfigList(cfglistview);

        // Item 详情信息
        ScrollView detailscrollview = root.Q<ScrollView>("scrolldetail");
        this.m_scrolldetailinfo = detailscrollview;
        detailscrollview.Add(new Label("空"));

        // 可拖动Line
        VisualElement lineelem = root.Q<VisualElement>("line");
        InitDragLine(lineelem);

        // 按钮
        GroupBox btngroup = root.Q<GroupBox>("btns");
        this.temp_btnsgroup = btngroup;
        InitBtnsGroup(btngroup);

        // 开始异步
        CoroutineEditorEX.Start(this.CreateGUIEndAsyn());
    }


    private void Awake()
    {
        this.m_userdata = ScriptableObject.CreateInstance<LevelData>();
        this.m_current_level = new Level();

        /* log 超链接点击 */
        EditorGUI.hyperLinkClicked += OnClickLogLink;
    }

    private void OnClickLogLink(EditorWindow arg1, HyperLinkClickedEventArgs arg2)
    {
        var mykey = "wgz";
        if (arg2.hyperLinkData.ContainsKey(mykey))
        {
            var file = arg2.hyperLinkData[mykey];
            var path_windows = Path.GetFullPath(file);
            /* 打开并选中文件 */
            string argument = "/select, \"" + path_windows + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }
    }

    private void OnHierarchyChange()
    {
        Debug.Log("OnHierarchyChange");
    }

    private void OnInspectorUpdate()
    {
        // listview
        /*
        var windows_height = rootVisualElement.layout.height;
        if (!m_hexagonlist.style.height.Equals(windows_height))
            m_hexagonlist.style.height = windows_height;
        if (!m_cfglist.style.height.Equals(windows_height))
            m_cfglist.style.height = windows_height;
        */

        // groupbox
        /*
        if (temp_middle == null)
            temp_middle = rootVisualElement.Q<GroupBox>("middle");
        if (temp_detail == null)
            temp_detail = rootVisualElement.Q<GroupBox>("detail");
        temp_middle.style.height = windows_height * 0.7f;
        temp_detail.style.height = windows_height * 0.3f;
        temp_detail.style.top = windows_height * 0.7f;
        */

        // btns
        /*
        if (temp_btns == null)
            temp_btns = rootVisualElement.Q<GroupBox>("btns");
        var oldheight = temp_btns.layout.height;
        temp_btns.style.top = windows_height - oldheight; 
        */
    }

    private void OnGUI()
    {

    }


    private void OnValidate()
    {
        Debug.Log("OnValidate");
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
        EditorGUI.hyperLinkClicked -= OnClickLogLink;
    }


    private IEnumerator CreateGUIEndAsyn()
    {
        yield return null;
        var scrollcontent = this.m_scrollcontent;
        /* var viewport = scrollconten.contentViewport; */
        scrollcontent.RegisterCallback<MouseDownEvent>(OnMouseDown);
        scrollcontent.RegisterCallback<MouseMoveEvent>(OnMoussMove);
        scrollcontent.RegisterCallback<MouseUpEvent>(OnMouseUp);
        scrollcontent.RegisterCallback<MouseLeaveEvent>(OnMouseUp);
    }

    private void OnMouseUp(MouseLeaveEvent evt)
    {
        this.m_scrollcontent_iscan = false;
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        this.m_scrollcontent_iscan = false;
    }

    private void OnMoussMove(MouseMoveEvent evt)
    {
        if (!this.m_scrollcontent_iscan) return;
        var scrollscontent = this.m_scrollcontent;
        var content = scrollscontent.contentContainer;
        var viewport = scrollscontent.contentViewport;

        /*
        var xmin = viewport.contentRect.width - content.contentRect.width;
        var ymin = viewport.contentRect.height - content.contentRect.height;
        if (xmin > 0) xmin = 0;
        if (ymin > 0) ymin = 0;

        Vector3 oldVec3 = content.transform.position;
        content.transform.position = new Vector2(
                Mathf.Clamp(oldVec3.x + evt.mouseDelta.x * m_scrollconten_speedrate, xmin, 0),
                Mathf.Clamp(oldVec3.y + evt.mouseDelta.y * m_scrollconten_speedrate, ymin, 0));
        */

        var xmax = content.contentRect.width - viewport.contentRect.width;
        var ymax = content.contentRect.height - viewport.contentRect.height;
        if (xmax < 0) xmax = 0;
        if (ymax < 0) ymax = 0;

        Vector2 oldvec2 = scrollscontent.scrollOffset;
        scrollscontent.scrollOffset = new Vector2(
                Mathf.Clamp(oldvec2.x - evt.mouseDelta.x * m_scrollconten_speedrate, 0, xmax),
                Mathf.Clamp(oldvec2.y - evt.mouseDelta.y * m_scrollconten_speedrate, 0, ymax));

        //Debug.Log($"localposition:{content.transform.position}  scrolloffset:{scrollscontent.scrollOffset}");
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button != 2) return; // 仅鼠标中间有效
        this.m_scrollcontent_iscan = true;
    }

    /* 按钮组 */
    private void InitBtnsGroup(GroupBox btngroup)
    {
        var submit = btngroup.Q<Button>("submit");
        submit.clicked += () =>
        {
            var jsonpath = EditorUtility.SaveFilePanel("保存Json文件", EditorApplication.applicationPath, "newlevel", "json");
            if (string.IsNullOrEmpty(jsonpath)) return; // 取消保存

            var newlevel = this.m_current_level;
            newlevel.Check();
            var strdata = newlevel.ToJson();
            File.WriteAllText(jsonpath, strdata, System.Text.Encoding.UTF8);
            Debug.Log($"<color=yellow>导出完成</color>=><a wgz=\"{jsonpath}\">{jsonpath}</a>");
        };
    }


    /*六边形颜色列表 */
    private void InitHexagonList(ListView listView)
    {
        // 读取数据
        List<HexagonVisualElement.ParamVariable> elements = new List<HexagonVisualElement.ParamVariable>(MyExtend.ColorArray.Length);
        var width = 50;
        var height = 43.3f;

        for (int i = 0; i < MyExtend.ColorArray.Length; i++)
        {
            var color = MyExtend.ColorArray[i];
            HexagonVisualElement.ParamVariable param = new HexagonVisualElement.ParamVariable(width, height, color, i);
            param.outlineWidth = 1f;
            elements.Add(param);
        }

        // Bind 数据列表
        int count = 0;
        listView.itemsSource = elements;
        listView.makeItem = () =>
        {
            var item = elements[count];
            HexagonVisualElement hexagonVisualElement = new HexagonVisualElement(item.mainIndex);
            hexagonVisualElement.Init(item);
            count++;
            return hexagonVisualElement;
        };
        listView.bindItem = (elem, index) => { };

        // 事件绑定
        listView.onSelectionChange += (items) =>
        {
            var elemdata = items.First() as HexagonVisualElement.ParamVariable;
            HexagonVisualElement hexagonVisualElement = new HexagonVisualElement(elemdata.mainIndex);
            hexagonVisualElement.Init(elemdata);

            var index = listView.selectedIndex;
            var elemcontent = listView.hierarchy[0].contentContainer;
            var elem = elemcontent[index];
            rootVisualElement.Add(hexagonVisualElement);

            var vec2_local = hexagonVisualElement.WorldToLocal(elem.LocalToWorld(elem.transform.position));
            hexagonVisualElement.style.position = Position.Absolute;
            hexagonVisualElement.transform.position = vec2_local;
            this.m_dragAndDropManipulator.target = hexagonVisualElement;

            // 触发事件
            using (var pointerdown = new PointerDownEvent())
            {
                /*
                pointerdown.target = hexagonVisualElement;
                hexagonVisualElement.SendEvent(pointerdown);
                */
                this.m_dragAndDropManipulator.PointerDownHandlerUser(pointerdown);
            }

        };


    }

    /* 配置列表 */
    private void InitConfigList(ListView cfglistview)
    {
        string jsonpath = "/Editor/Test/Cfg";
        string path = Application.dataPath + jsonpath;

        // json文件列表
        DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*.json");
        string[] names = new string[fileInfos.Length];
        for (int i = 0; i < fileInfos.Length; i++)
        {
            var info = fileInfos[i];
            names[i] = Path.GetFileNameWithoutExtension(info.Name);
        }

        // View更新
        int indexcount = 0;
        cfglistview.itemsSource = names;
        cfglistview.makeItem = () =>
        {
            Label label = new Label();
            label.text = names[indexcount];
            indexcount++;

            // style
            var lable_style = label.style;
            lable_style.minWidth = 150;
            lable_style.minHeight = 30;
            lable_style.unityTextAlign = TextAnchor.MiddleCenter;


            return label;
        };
        cfglistview.bindItem = (elem, index) => { };
    }



    /* 初始化内容区 */
    private void InitScrollContent(ScrollView scrollView)
    {
        var totalwidth = 800;
        var totalheight = 600;
        float itemwidth = 50;
        float itemheight = 43.3f;

        var offsetLeft = itemwidth * m_percent_hexagon[1] * -1;
        var offsetTop = itemheight * m_percent_hexagon[2];
        int column = Mathf.FloorToInt((totalwidth - (m_percent_hexagon[1] * itemwidth)) / (itemwidth * m_percent_hexagon[3]));
        int row = Mathf.RoundToInt(totalheight / itemheight);
        var scrollview = this.m_scrollcontent;

        // Init data
        this.m_totalItemdata = new ItemData[column, row];
        var num = column % 2 == 0 ? column - 1 : column;
        var count = Mathf.FloorToInt(num / 2);
        this.m_totalItemdata_oblique = new ItemData[row + count][];

        // Item 表现设置
        Color color = Color.white; // 底板颜色
        color.a = 0.75f;
        HexagonVisualElement.ParamVariable paramVariable = new HexagonVisualElement.ParamVariable(itemwidth, itemheight, color, 0);
        paramVariable.outlineWidth = 1f;

        for (int i = 0; i < column; i++)
        {
            var sign = i % 2;
            VisualElement visualElement = new VisualElement();
            visualElement.name = "column";

            visualElement.style.minWidth = itemwidth;
            visualElement.style.maxWidth = itemwidth;
            visualElement.style.width = itemwidth;
            visualElement.style.height = totalheight;
            visualElement.style.minHeight = totalheight;
            visualElement.style.maxHeight = totalheight;

            for (int j = 0; j < row; j++)
            {
                // 定位数据
                Vector2Int vec2int = new Vector2Int(i, j);

                HexagonVisualElement hexagonVisualElement = new HexagonVisualElement(vec2int);
                hexagonVisualElement.name = "item";
                hexagonVisualElement.Init(paramVariable);
                visualElement.Add(hexagonVisualElement);

                hexagonVisualElement.style.minWidth = itemwidth;
                hexagonVisualElement.style.maxWidth = itemwidth;
                hexagonVisualElement.style.width = itemwidth;
                hexagonVisualElement.style.height = itemheight;
                hexagonVisualElement.style.minHeight = itemheight;
                hexagonVisualElement.style.maxHeight = itemheight;

                /* 添加右键菜单项 */
                SetContextualMenu(hexagonVisualElement);

                /* 鼠标左键点击事件 */
                hexagonVisualElement.RegisterCallback<ClickEvent>((evt) => this.OnClickBaseHexagon(evt, hexagonVisualElement));

                /* Add数据 */
                var dic = this.m_totalItemdata;
                dic[i, j] = new ItemData(vec2int, hexagonVisualElement);
            }

            if (sign > 0)
            { visualElement.style.top = offsetTop; }
            visualElement.style.left = offsetLeft * i;
            scrollView.Add(visualElement);
        }

        /* 整理倾斜坐标数据 */
        CalculateObliqueData(column, row);

        // Content布局微调
        CoroutineEditorEX.Start(InitScrollContentEndAsyn(column, itemwidth));
    }



    // ItemgGround Click事件
    private void OnClickBaseHexagon(ClickEvent evt, HexagonVisualElement element)
    {
        var pos = element.position;
        var dic = this.m_totalItemdata;
        var itemdata = dic[pos.x, pos.y];
        var detailElem = this.m_scrolldetailinfo;

        // 清空已有的
        detailElem.Clear();

        var leveldata = this.m_userdata;
        SerializedObject serializedObject = new SerializedObject(leveldata);
        SerializedProperty base_target = serializedObject.FindProperty("m_base_target");
        PropertyField propertyField = new PropertyField(base_target, "目标");
        propertyField.BindProperty(serializedObject);
        detailElem.Add(propertyField);

        // 添加新建项
        if (itemdata.isHaveElemGround)
        {
            leveldata.SetItemData(itemdata);

            var vec2 = serializedObject.FindProperty("m_vec2");
            var base_type = serializedObject.FindProperty("m_base_type");
            var base_value = serializedObject.FindProperty("m_base_value");
            var type = serializedObject.FindProperty("m_type");
            var values = serializedObject.FindProperty("m_values");
            var valuestuples = serializedObject.FindProperty("m_values_dic");

            PropertyField vec2_field = new PropertyField(vec2, "位置");
            vec2_field.BindProperty(serializedObject);

            PropertyField base_type_field = new PropertyField(base_type, "类型[底]");
            base_type_field.BindProperty(serializedObject);
            PropertyField base_value_field = new PropertyField(base_value, "值");
            base_value_field.BindProperty(serializedObject);

            var linespace = new VisualElement();
            linespace.style.height = 16;
            detailElem.Add(vec2_field);
            detailElem.Add(linespace);
            detailElem.Add(base_type_field);
            detailElem.Add(base_value_field);

            /* 当列表有数据时才计算 */
            if (itemdata.isHaveElemList)
            {
                PropertyField type_field = new PropertyField(type, "类型[层]");
                type_field.BindProperty(serializedObject);
                PropertyField values_field = new PropertyField(values, "值[ArrayInt]");
                values_field.BindProperty(serializedObject);
                detailElem.Add(type_field);
                detailElem.Add(values_field);

                var reorderableList = new ReorderableList(serializedObject, valuestuples, false, true, false, false);
                reorderableList.drawHeaderCallback = (rect) => GUI.Label(rect, "列表");
                reorderableList.elementHeightCallback = (index) =>
                { return EditorGUI.GetPropertyHeight(valuestuples.GetArrayElementAtIndex(index)); };
                reorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    var label = EditorGUIUtility.TrTextContent(string.Format("第{0}层,颜色,计数", index + 1));
                    var item = valuestuples.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, item, label);
                };
                reorderableList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                { GUI.Box(rect, "", EditorStyles.miniButton); };

                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    serializedObject.Update();
                    reorderableList.DoLayoutList();
                    if (serializedObject.hasModifiedProperties)
                        serializedObject.ApplyModifiedProperties();
                });
                detailElem.Add(container);
            }
        }

    }

    // 当再次拖动元素时
    private void OnDragVisualElem(Vector2Int obj, HexagonVisualElement elem)
    {
        var dic = this.m_totalItemdata;
        var itemdata = dic[obj.x, obj.y];

        if (itemdata.isHaveElemList)
        {
            // 移除最后添加的，即最高层的
            var elems = itemdata.elements;
            if (elems.Contains(elem)) elems.Remove(elem);
        }
        else if (itemdata.isHaveElemGround)
        { itemdata.elementGround = null; }
    }

    // 当放置元素时
    private void OnDropVisualElem(Vector2Int obj, HexagonVisualElement elem)
    {
        var dic = this.m_totalItemdata;
        var itemdata = dic[obj.x, obj.y];

        if (!itemdata.isHaveElemGround) itemdata.elementGround = elem;
        else { itemdata.elements.Add(elem); }

        /* 每次拖放后计算新布局的坐标 */
        this.CalculatePosition();
    }


    // 计算序列化数据Level数据坐标
    private void CalculatePosition()
    {
        var dic = this.m_totalItemdata;
        var dic_oblique = this.m_totalItemdata_oblique;

        // 坐标计算开始列
        var startcolumn = -1;
        foreach (var item in dic)
        {
            if (item.isHaveElemGround)
            { startcolumn = item.posv2.x; break; }
        }

        // 坐标计算需要的斜坐标数据
        var posint2 = Vector2Int.one * -1;
        var indexdoublearr = -1;
        for (int i = 0; i < dic_oblique.Length; i++)
        {
            var arr = dic_oblique[i];
            var isbreak = false;
            for (int j = arr.Length - 1; j >= 0; j--)
            {
                var item = arr[j];
                if (item.isHaveElemGround)
                {
                    posint2 = item.posv2;
                    isbreak = true;
                    indexdoublearr = i;
                    break;
                }
            }
            if (isbreak) break;
        }

        // 判空检查
        if (startcolumn < 0 || posint2.x < 0)
        { Debug.LogWarning("面板可能为空请假查"); return; }

        var level = this.m_current_level;
        level.target = this.m_userdata.target;
        level.grounds_out.Clear();
        level.inits_out.Clear();

        // 计算定位坐标
        for (int i = indexdoublearr; i < dic_oblique.Length; i++)
        {
            var x = i - indexdoublearr;
            var arr = dic_oblique[i];
            var temp = 0;
            for (int j = arr.Length - 1; j >= 0; j--)
            {
                var item = arr[j];
                if (item.posv2.x < startcolumn) continue;
                else if (item.posv2.x == startcolumn) temp = arr.Length - (j + 1); // 到x=startcolumn元素的距离

                if (item.isHaveElemGround)
                {
                    var y = (arr.Length - (j + 1)) - temp; // 计算Y的索引，从x=startcolumn开始，x置0
                    item.ground.x = x;
                    item.ground.y = y;
                    item.specialGround.x = x;
                    item.specialGround.y = y;

                    /* 添加序列化数据 */
                    level.grounds_out.Add(item.ground);
                    if (item.isHaveElemList)
                        level.inits_out.Add(item.specialGround);
                }
            }
        }


    }


    // 计算倾斜坐标数据
    private void CalculateObliqueData(int column, int row)
    {
        var dic = this.m_totalItemdata;
        var dic_oblique = this.m_totalItemdata_oblique;


        /* part up */
        for (int i = 0; i < row; i++)
        {
            var len = 2 * i + 1;
            var offset = column - len;
            if (offset < 0) len += offset;

            dic_oblique[i] = new ItemData[len];
            var k = i;
            for (int j = 0; j < len; j++)
            {
                if (j % 2 != 0) k--;
                var index_start = column - 1;
                var l = index_start - j;
                dic_oblique[i][j] = dic[l, k];
            }
        }


        /* part down */ /* 次数多减1是因为上述已经处理完了最后1列 */
        var num = (column - 1) / 2;
        for (int i = 0; i < num; i++)
        {
            var len = column - 1 - 2 * i;
            var n = row + i;
            var k = row;
            dic_oblique[n] = new ItemData[len];

            for (int j = 0; j < len; j++)
            {
                if (j % 2 == 0) k--;
                dic_oblique[n][j] = dic[len - 1 - j, k];
            }
        }

    }


    // ItenGround 右键菜单逻辑
    private void SetContextualMenu(HexagonVisualElement element)
    {
        ContextualMenuManipulator manipulator = new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
        {
            evt.menu.AppendAction("Clear", (DropdownMenuAction obj) =>
            {
                var items = element.Children();
                var len = items.Count();
                if (len <= 0) return;

                // delete visual elem
                for (int i = 0; i < len; i++)
                {
                    var item = items.ElementAt(0);
                    element.Remove(item);
                }

                // delete cache data
                var dic = this.m_totalItemdata;
                dic[element.position.x, element.position.y].Clear();
            });
        });
        manipulator.target = element;
    }


    // 异步反射设定Content区宽高
    private IEnumerator InitScrollContentEndAsyn(int column, float itemwidth)
    {
        yield return null;

        /* scrollcontent set height and width */
        var contentElem = m_scrollcontent.contentContainer;
        var oldwidth = contentElem.contentRect.width;
        var offsetwidth = (column - 1) * itemwidth * m_percent_hexagon[1]; // 1/4宽度
        var boundingBox = (Rect)(typeof(VisualElement).GetProperty("boundingBox", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(contentElem));

        if (boundingBox != null)
        {
            contentElem.style.maxWidth = boundingBox.width - offsetwidth;
            contentElem.style.minWidth = boundingBox.width - offsetwidth;
            contentElem.style.width = boundingBox.width - offsetwidth;

            contentElem.style.maxHeight = boundingBox.height;
            contentElem.style.minHeight = boundingBox.height;
            contentElem.style.height = boundingBox.height;
        }
    }




    /* 初始化详情item信息 */
    private void InitDetailInfoList(ScrollView scrollView)
    {
        SerializedObject serializedObject = new SerializedObject(m_userdata);

        var prop = serializedObject.FindProperty("m_baseground_solo");
        var propertyField = new PropertyField(prop);
        propertyField.BindProperty(serializedObject);
        scrollView.Add(propertyField);

        var proparr = serializedObject.FindProperty("m_specialground_arr");
        var propertyarrField = new PropertyField(proparr);
        propertyarrField.BindProperty(serializedObject);
        scrollView.Add(propertyarrField);
    }


    /* 可拖动线条 */
    private void InitDragLine(VisualElement lineelem)
    {
        var insideline = lineelem.Q<VisualElement>("lineinside");

        Color linecol = Color.white;
        insideline.RegisterCallback<MouseEnterEvent>((evt) =>
        {   // 高亮线条
            linecol.a = 1;
            lineelem.style.backgroundColor = linecol;
            SetNewSystemCursor();
        });
        insideline.RegisterCallback<MouseLeaveEvent>((evt) =>
        {   // 恢复线条
            linecol.a = 0;
            lineelem.style.backgroundColor = linecol;
            ResetSystemCursor();
        });

        // 移动鼠标时
        bool ismove = false;
        float oldheight = 0; // 此偏移值为负值
        float totalheight = 0;
        var windows = rootVisualElement.parent;

        insideline.RegisterCallback<MouseDownEvent>((evt) =>
        {
            ismove = true;
            oldheight = lineelem.resolvedStyle.bottom;
            totalheight = lineelem.parent.layout.height;
        });


        windows.RegisterCallback<MouseUpEvent>((evt) =>
        {
            if (ismove)
            {
                ismove = false;
                oldheight = 0;
                totalheight = 0;
            }
        });
        windows.RegisterCallback<MouseMoveEvent>((evt) =>
        {
            if (!ismove) return;

            // 线条百分比
            oldheight += evt.mouseDelta.y;
            float rate = Mathf.Abs(oldheight) / totalheight;
            Length length = rate * 100;
            length.unit = LengthUnit.Percent;
            lineelem.style.bottom = length;

            // 上下部分布局百分比
            this.m_scrollcontent.parent.style.bottom = length;
            Length length2 = length;
            length2.value = (1 - rate) * 100;
            this.m_scrolldetailinfo.parent.style.top = length2;
        });


    }



    #region Cursor

    /* Set System Cursor */
    [DllImport("User32.DLL")]
    public static extern bool SetSystemCursor(IntPtr hcur, uint id);
    public const uint OCR_NORMAL = 32512;

    [DllImport("User32.DLL")]
    public static extern IntPtr LoadCursorFromFile(string fileName);

    [DllImport("User32.DLL")]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
    public const uint SPI_SETCURSORS = 87;
    public const uint SPIF_SENDWININICHANGE = 2;

    private void SetNewSystemCursor()
    {
        var windowsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        IntPtr newcursor = LoadCursorFromFile(windowsfolder + @"\Cursors\aero_ns.cur");
        SetSystemCursor(newcursor, OCR_NORMAL);
    }

    private void ResetSystemCursor()
    {
        SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_SENDWININICHANGE);
    }

    #endregion




    /* 编辑器测试用 */
    [MenuItem("Tool/Test")]
    public static void Test()
    {
        /*
        Debug.Log("Click me: <a href=\"Assets/Editor/ConditionalFieldAttribute.cs\" line=\"2\">local file</a>");
        EditorGUI.hyperLinkClicked += (window, args)
            => Debug.Log($"clicked link to {args.hyperLinkData["href"]} in {window}");
        */
    }

}