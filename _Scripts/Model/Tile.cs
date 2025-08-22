
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool IsObstacle;
    public List<Unit> occupyingUnits = new List<Unit>();
    public int MaxUnitsPerTile => 9;

    public void Init(int x, int y)
    {
        gridPosition = new Vector2Int(x, y);
    }

    public bool IsOccupied => occupyingUnits.Count >= MaxUnitsPerTile;

    public void SetOccupied(Unit unit)
    {
        if (unit == null) return;
        // tránh add duplicate và chỉ add khi còn chỗ
        if (!occupyingUnits.Contains(unit) && occupyingUnits.Count < MaxUnitsPerTile)
            occupyingUnits.Add(unit);
    }

    public void SetUnoccupied(Unit unit)
    {
        if (unit == null) return;
        // remove tất cả các entry trùng (phòng duplicate bug)
        occupyingUnits.RemoveAll(u => u == unit || u == null);
    }

    public Vector3 GetLocalOffsetForUnit(int index)
    {
        int row = index / 3;
        int col = index % 3;
        float spacing = 1f; 
        return new Vector3((col - 1) * spacing, (row - 1) * spacing, 0);
    }

    private void OnMouseDown()
    {
        Unit selected = Unit.GetSelectedUnit();
        if (selected == null) return;

        if (IsObstacle || occupyingUnits.Count >= MaxUnitsPerTile) return;

        Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);
        Vector3 offset = GetLocalOffsetForUnit(occupyingUnits.Count); 
        Vector3 newPos = basePos + offset;

        selected.MoveTo(newPos, gridPosition);
    }


}
