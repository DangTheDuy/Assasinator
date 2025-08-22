
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
        if (occupyingUnits.Count < MaxUnitsPerTile)
        {
            occupyingUnits.Add(unit);
        }
    }

    public void SetUnoccupied(Unit unit)
    {
        if (occupyingUnits.Contains(unit))
        {
            occupyingUnits.Remove(unit);
        }
    }

    public Vector3 GetLocalOffsetForUnit(int index)
    {
        // Tính offset theo lưới 3x3
        int row = index / 3;
        int col = index % 3;
        float spacing = 1f; // khoảng cách giữa các unit
        return new Vector3((col - 1) * spacing, (row - 1) * spacing, 0);
    }

    private void OnMouseDown()
    {
        Unit selected = Unit.GetSelectedUnit();
        if (selected != null)
        {
            if (!IsObstacle && occupyingUnits.Count < MaxUnitsPerTile)
            {
                // Giải phóng tile cũ
                Tile oldTile = GridManager.Instance.GetTileAtPosition(selected.currentPosition);
                if (oldTile != null)
                    oldTile.SetUnoccupied(selected);

                // Đặt unit vào tile mới
                Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);
                Vector3 offset = GetLocalOffsetForUnit(occupyingUnits.Count);
                Vector3 newPos = basePos + offset;

                selected.MoveTo(newPos, gridPosition);
                SetOccupied(selected);
            }
        }
    }


}
