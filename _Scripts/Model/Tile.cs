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
        float spacing = 0.5f; // khoảng cách giữa các unit
        return new Vector3((col - 1) * spacing, (row - 1) * spacing, 0);
    }
}



