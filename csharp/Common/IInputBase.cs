
using UnityEngine;

public interface IInputBase
{
    // 输入是否启用
    bool IsInputEnabled { get; set; }

    
    
    void OnPointerDown(DInputData inputData);
    void OnPointerUp(DInputData inputData);
    void OnPointerClick(DInputData inputData); 
    void OnBeginDrag(DInputData inputData);
    void OnEndDrag(DInputData inputData);
    void OnDrag(DInputData inputData);
}

public struct DInputData
{
    public Vector2 positon;
    public Vector2 pressPosition;
    public Vector2 delta;
    public bool dragging;
}
