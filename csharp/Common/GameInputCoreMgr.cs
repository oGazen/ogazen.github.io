using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using DG.Tweening;
using ModAropAway;
using URandom = UnityEngine.Random;

public class GameInputCoreMgr : MonoBehaviour,InterfaceInputBase
{
    private static GameInputCoreMgr instance;
    public static GameInputCoreMgr Ins => instance;
    
    public bool IsInputEnabled { get; set; }

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
    
    
    // ================================= 本地参数
    private int s_boxLayer;

    private bool m_isClickValid;
    private RaycastHit m_hitClosed;

    private Vector3 m_inputOffsetBox;
    private Vector3 m_targetPos;
    private bool m_isDrag;
    

    private BoxBrickMod m_boxBrickMod_current;
    private BoxBrickTrigger m_boxBrickTrigger_current;

    // ========================================== Unity 事件函数
    void Awake()
    {
        inside();
        s_boxLayer = 1 << LayerMask.NameToLayer("Box");
        m_isClickValid = false;
    }


    void Start()
    {
        DInputUIMgr.Ins.Register(this);
    }

    private void OnDestroy()
    {
        DInputUIMgr.Ins.UnRegister(this);
    }
    

    
    // ============================================== 输入函数
    public void OnPointerDown(DInputData inputData)
    {
        m_isClickValid = MUtility.RayCastToClosed(inputData.pressPosition, out m_hitClosed,layermask:s_boxLayer);
        if (m_isClickValid)
        {
            m_boxBrickMod_current = m_hitClosed.transform.GetComponentInParent<BoxBrickMod>();
            m_boxBrickTrigger_current = m_boxBrickMod_current.GetBoxBrickTrigger();
            
            m_isClickValid = !GameRuntimeDataMgr.Ins.IsValidConnectBoxBrickID(m_boxBrickMod_current.GetConnectBoxBrickID()) 
                             && !GameRuntimeDataMgr.Ins.IsPartCrateGridPos(m_boxBrickMod_current.GetGridPosCurrent());

            if (m_isClickValid)
            {
                DEventManager.Ins.Execute(DEventManager.E_OnBoxBrickPointerDown,m_boxBrickMod_current);
                
                Vector3 hitPlanePos = MUtility.GetPointForPlane(inputData.positon,Vector3.zero,Vector3.up);
                m_inputOffsetBox = m_boxBrickMod_current.transform.position - hitPlanePos; // 计算点击位置 与 方块的位置左边的 偏移
                
                GameRuntimeDataMgr.Ins.TriggerGameStart();

                m_boxBrickTrigger_current.SetTriggerMe(true);
        
                // DConsole.DebugLog($"TEST {m_boxBrickMod_current.gameObject.name} 有效位置检查 OnDown wgz++++++++++++++ 添加");
                GameRuntimeDataMgr.Ins.AddGridPosRange(m_boxBrickMod_current.GetCheckPointsAll());
            }
        }
    }

    public void OnPointerUp(DInputData inputData)
    {
        if (!m_isClickValid) return;


        m_isClickValid = false;
        m_isDrag = false;
        DEventManager.Ins.Execute(DEventManager.E_OnBoxBrickPointerUp,m_boxBrickMod_current);
        

        

        // 增加检查 防止空气墙
        if (m_boxBrickMod_current.GetIsValid())
        {
            // DConsole.DebugLog($"TEST {m_boxBrickMod_current.gameObject.name} 有效位置检查 OnUp wgz++++++++++++++ 删除");
            GameRuntimeDataMgr.Ins.DelGridPosRange(m_boxBrickMod_current.GetCheckPointsAll());
        }


        Vector3 pos = m_boxBrickMod_current.transform.localPosition;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.z = Mathf.RoundToInt(pos.z);
        m_boxBrickMod_current.transform.DOLocalMove(pos, 0.15f);
        
        
        m_boxBrickTrigger_current.SetTriggerMe(false);
    }

    public void OnPointerClick(DInputData inputData)
    {
        
    }

    public void OnBeginDrag(DInputData inputData)
    {
        
    }

    public void OnEndDrag(DInputData inputData)
    {
        
    }
    
    
    public void OnDrag(DInputData inputData)
    {
        if (!m_isClickValid) return;
        
        m_isDrag = true;
        Vector3 hitPlanePos = MUtility.GetPointForPlane(inputData.positon,Vector3.zero,Vector3.up);
        m_targetPos = hitPlanePos + m_inputOffsetBox;
    }


    private void FixedUpdate()
    {
        if(!m_isClickValid || !m_isDrag) return;
        
        
        var oldPos = m_boxBrickMod_current.GetWorldPos(); // 方块准确的世界坐标
        var checkPos = m_boxBrickMod_current.GetWorldPosForGridPos(); // 方块近似整数的世界坐标
        var targetPos = m_targetPos; // 准确的目标坐标
        var targetDir = targetPos - checkPos; // 移动方向 （此处可能会改变基准点）
        

        
        
        Vector2Int target_offset = Vector2Int.one;
        target_offset.x *= targetDir.x >= 0 ? 1 : -1;
        target_offset.y *= targetDir.z >= 0 ? 1 : -1;
        
        // IsMoveX,IsMoveZ,Slope
        ValueTuple<bool, bool, bool> check_result = default;
        CheckCore(target_offset,targetDir,ref check_result);
        
        
        
        if (!check_result.Item1)
        {
            targetPos.x = checkPos.x;
            if (check_result.Item3)
            {
                var slopePos = checkPos;
                slopePos.z = oldPos.z;
                var localPos_slope = Vector3.Lerp(oldPos, slopePos, Time.deltaTime * 25);
                m_boxBrickMod_current.transform.localPosition = localPos_slope;
            }
        }
        
        if (!check_result.Item2)
        {
            targetPos.z = checkPos.z;
            if (check_result.Item3)
            {
                var slopePos = checkPos;
                slopePos.x = oldPos.x;
                var localPos_slope = Vector3.Lerp(oldPos, slopePos, Time.deltaTime * 25);
                m_boxBrickMod_current.transform.localPosition = localPos_slope;
            }
        }


        oldPos = m_boxBrickMod_current.GetWorldPos();
        var localPosNew = Vector3.Lerp(oldPos, targetPos, Time.fixedDeltaTime * 25);
        var localLerpPos = localPosNew - oldPos;
        float deltaMax = check_result.Item3 ? 0.05f : 0.85f;
        if (localLerpPos.magnitude > deltaMax)
        {
            localPosNew = oldPos + localLerpPos.normalized * deltaMax;
        }
        
        
        m_boxBrickMod_current.transform.localPosition = localPosNew;
    }


    
    private void CheckCore(Vector2Int offset,Vector3 delta,ref ValueTuple<bool,bool,bool> check_result)
    {
        
        // 水平 垂直检查
        Vector2Int offset_delta = default;
        float delta_x = Mathf.Abs(delta.x);
        float delta_z = Mathf.Abs(delta.z);

        offset_delta.x = offset.x;
        offset_delta.y = 0;
        check_result.Item1 = CheckBoxMoveTargetIsValid(offset_delta);

        offset_delta.x = 0;
        offset_delta.y = offset.y;
        check_result.Item2 = CheckBoxMoveTargetIsValid(offset_delta);


        check_result.Item3 = false;
        
        
        // 锁轴
        switch (m_boxBrickMod_current.GetAxisLockType())
        {
            case AxisLockType.Vertical:
                check_result.Item1 = false;
                return;
            case AxisLockType.Horizontal:
                check_result.Item2 = false;
                return;
        }

        
        
        
        // 斜向
        if (check_result is { Item1: true, Item2: true })
        {
            bool isAnyMove = CheckBoxMoveTargetIsValid(offset);
            if (!isAnyMove)
            {
                bool isX = delta_x >= delta_z;
                check_result.Item1 = isX;
                check_result.Item2 = !isX;

                
                var oldPos = m_boxBrickMod_current.GetWorldPos(); // 方块准确的世界坐标
                var checkPos = m_boxBrickMod_current.GetWorldPosForGridPos(); // 方块近似整数的世界坐标
                
                if (isX)
                {
                    float x_distance = oldPos.x - checkPos.x;
                    if (x_distance is > -0.05f and < 0.05f)
                    {
                        check_result.Item3 = true;
                    }
                }
                else
                {
                    float z_distance = oldPos.z - checkPos.z;
                    if (z_distance is > -0.05f and < 0.05f)
                    {
                        check_result.Item3 = true;
                    }
                }
            }
        }



    }
    
    
    
    private bool CheckBoxMoveTargetIsValid(Vector2Int offset)
    {
        // 挡板类型检查
        bool isShapeCan_first = true;
        bool isShapeCan_second = true;
        Vector3 shapeV3 = Vector3.zero;
        if (offset.x != 0)
        {
            shapeV3.x = offset.x;
            isShapeCan_first = m_boxBrickMod_current.IsShapeCanMoveDirection(shapeV3);
        }
        
        if (offset.y != 0)
        {
            shapeV3.x = 0;
            shapeV3.z = offset.y;
            isShapeCan_first &= m_boxBrickMod_current.IsShapeCanMoveDirection(shapeV3);
        }
        
        
        
        // 当前检查节点
        var refPoints_current = m_boxBrickMod_current.GetCheckPointsAll();

        
        // 判断的BoxBrick的ColorIndex
        var box_colorindex = m_boxBrickMod_current.GetColorIndex();
        
        
        // 嵌套类型
        if (m_boxBrickMod_current.IsHaveNestedBox())
        {
            box_colorindex = m_boxBrickMod_current.GetNestedBoxBrick().GetColorIndex();
        }


        int firstBox_check = 0;
        for (int i = 0; i < refPoints_current.Length; i++)
        {
            var item_first = refPoints_current[i];
            var item_checkPos = item_first + offset;
            
            
            
            bool item_first_isboard = GameRuntimeDataMgr.Ins.IsValidGridPos(item_checkPos);
            
            PersonMode personMode_checkPos = GameRuntimeDataMgr.Ins.GetPersonMode(item_checkPos);
            bool item_first_iscolor = personMode_checkPos != null 
                                      && personMode_checkPos.GetColorIndex() == box_colorindex 
                                      && !GameRuntimeDataMgr.Ins.IsPartCrateGridPos(item_checkPos) 
                                      && isShapeCan_first;

            PlugMan plugMan = GameRuntimeDataMgr.Ins.GetPlugMan(item_checkPos);
            bool plugman_check = plugMan != null
                                 && m_boxBrickMod_current.IsValidPlugOutlet()
                                 && plugMan.GetPlugOutletColor() == m_boxBrickMod_current.GetPlugOutletColor();
            
            // DConsole.DebugLog($"TEST wgz+++++++++++++++++ 检查 {item_checkPos}： {personMode_checkPos?.GetColorIndex()}-{box_colorindex}");
            // DConsole.DebugLog($"TEST wgz+++++++++++++++++ 检查 {item_checkPos}： {item_first_isboard}-{item_first_iscolor}-{plugman_check}");
            if (item_first_isboard || item_first_iscolor || plugman_check)
            {
                firstBox_check++;
            }
            else
            {
                break;
            }
        }

        bool isAnyMove_first = firstBox_check == refPoints_current.Length;

        
        return isAnyMove_first;
    }
    
    
    
    
}




