using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class DragAndDropManipulator : PointerManipulator
{
    public DragAndDropManipulator(VisualElement rootElem, VisualElement rootTarget, System.Action<Vector2Int, HexagonVisualElement> dropaction, System.Action<Vector2Int, HexagonVisualElement> dragaction)
    {
        this.root = rootElem;
        this.root_target = rootTarget;
        this.dropAction = dropaction;
        this.dragAction = dragaction;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        // Register the four callbacks on target.
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        // Un-register the four callbacks from target.
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    private Vector2 targetStartPosition { get; set; }

    private Vector3 pointerStartPosition { get; set; }

    private bool enabled { get; set; }

    private VisualElement root { get; }
    private VisualElement root_target { get; }


    // 回调函数，用于处理数据
    private System.Action<Vector2Int, HexagonVisualElement> dropAction;
    private System.Action<Vector2Int, HexagonVisualElement> dragAction;


    private void PointerDownHandler(PointerDownEvent evt)
    {
        var target_hexvisualelem = target.parent as HexagonVisualElement;
        if (target_hexvisualelem != root_target)
        {
            Vector2 newvec2 = RootSpaceOfSlot(target);
            target.parent.Remove(target);
            root_target.Add(target);
            target.transform.position = newvec2;

            targetStartPosition = newvec2;
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            enabled = true;

            // 执行action
            if (dragAction != null) dragAction(target_hexvisualelem.position, target as HexagonVisualElement);
        }
    }

    public void PointerDownHandlerUser(PointerDownEvent evt)
    {
        targetStartPosition = target.transform.position;
        pointerStartPosition = Event.current.mousePosition;
        target.CapturePointer(evt.pointerId);
        enabled = true;
    }


    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;
            var targetroot = this.root_target;
            var xmax = targetroot.contentRect.width - target.contentRect.width;
            var ymax = targetroot.contentRect.height - target.contentRect.height;
            target.transform.position = new Vector2(
                Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, xmax),
                Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, ymax));
        }
    }



    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }


    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        if (enabled)
        {
            // Columu List
            var column_list = root.Query<VisualElement>("column");
            var overlappingSlots = column_list.Where(OverlapsTarget);

            // All Items List
            List<VisualElement> temp_items = new List<VisualElement>();
            var temp_list = overlappingSlots.ToList();
            for (int i = 0; i < temp_list.Count; i++)
            {
                var items = temp_list[i].Children();
                temp_items.AddRange(items);
            }

            // 判断检测
            if (temp_items.Count > 0)
            {
                var target_parent = FindNearestElem(temp_items) as HexagonVisualElement;
                var len = target_parent.childCount;

                // 2D 层级偏移
                Vector2 vec2 = new Vector2(-1f, -1f);
                target.transform.position = vec2 * len;
                target_parent.Add(target);

                // 执行回调
                if (dropAction != null) dropAction(target_parent.position, target as HexagonVisualElement);
            }
            else target.parent.Remove(target);
            enabled = false;
        }
    }


    private bool OverlapsTarget(VisualElement slot)
    {
        bool isHave = target.worldBound.Overlaps(slot.worldBound);
        return isHave;
    }



    private VisualElement FindNearestElem(IEnumerable<VisualElement> visualElements)
    {
        float bestDistanceSq = float.MaxValue;
        VisualElement closest = null;
        var mousevec2 = root_target.WorldToLocal(Event.current.mousePosition);

        foreach (VisualElement item in visualElements)
        {
            var centeroffset2 = new Vector2(item.layout.width / 2, item.layout.height / 2);
            var targetcenter2 = RootSpaceOfSlot(item);
            targetcenter2.x += centeroffset2.x;
            targetcenter2.y += centeroffset2.y;

            // 相对于 root_target 的鼠标坐标 - item中心的坐标
            Vector2 displacement = mousevec2 - targetcenter2;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            { bestDistanceSq = distanceSq; closest = item; }
        }
        return closest;
    }



    private Vector2 RootSpaceOfSlot(VisualElement slot)
    {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return root_target.WorldToLocal(slotWorldSpace);
    }


}
