
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
    private Dictionary<Unit, int> unitSlots = new Dictionary<Unit, int>();
    public int MaxUnitsPerTile => 9;
    private bool detectionCheckedThisFrame = false;

    void LateUpdate()
    {
        detectionCheckedThisFrame = false;
    }

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
        int slot = FindAvailableSlot();
        unitSlots[unit] = slot;

        bool hasHero = occupyingUnits.Exists(u => u is HeroUnit);
        bool hasEnemy = occupyingUnits.Exists(u => u is EnemyUnit);

        if (hasHero && hasEnemy && !detectionCheckedThisFrame)
        {
            detectionCheckedThisFrame = true;
            CheckDetection();
        }
    }

    public void SetUnoccupied(Unit unit)
    {
        if (unit == null) return;
        // remove tất cả các entry trùng (phòng duplicate bug)
        occupyingUnits.RemoveAll(u => u == unit || u == null);
        if (unitSlots.ContainsKey(unit))
            unitSlots.Remove(unit);
    }

    public Vector3 GetLocalOffsetForUnit(Unit unit)
    {
        if (unit == null || !unitSlots.ContainsKey(unit))
            return Vector3.zero;

        int index = unitSlots[unit];
        int row = index / 3;
        int col = index % 3;
        float spacing = 1f;
        return new Vector3((col - 1) * spacing, (row - 1) * spacing, 0);
    }

    private int FindAvailableSlot()
    {
        for (int i = 0; i < MaxUnitsPerTile; i++)
        {
            bool used = false;
            foreach (var kv in unitSlots)
            {
                if (kv.Value == i) { used = true; break; }
            }
            if (!used) return i;
        }
        return 0;
    }

    private void OnMouseDown()
    {
        Unit selected = Unit.GetSelectedUnit();
        if (selected == null) return;
        if (selected.currentPosition == gridPosition) return;

        if (!IsObstacle && occupyingUnits.Count < MaxUnitsPerTile)
        {
            // chỉ tính basePos
            Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);
            // MoveTo sẽ lo SetOccupied + offset
            selected.MoveTo(basePos, gridPosition);
        }
    }

    public void PlaceUnit(Unit unit)
    {
        if (unit == null) return;

        SetOccupied(unit); // gán slot cho unit này
        Vector3 offset = GetLocalOffsetForUnit(unit);
        Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);

        unit.transform.position = new Vector3(basePos.x + offset.x, basePos.y + offset.y, -1f);
        unit.currentPosition = gridPosition;
    }
    
    public void CheckDetection()
    {
        // ✅ lấy % phát hiện cao nhất trong tile
        int highestDetectChance = 0;
        foreach (var unit in occupyingUnits)
        {
            if (unit is EnemyUnit enemy)
            {
                highestDetectChance = Mathf.Max(highestDetectChance, enemy.DetectionChance);
            }
        }

        // Gọi detection roll
        if (highestDetectChance > 0)
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < highestDetectChance)
            {
                Debug.Log($"Hero bị phát hiện! (roll {roll}/{highestDetectChance})");
            }
            else
            {
                Debug.Log($"Hero chưa bị phát hiện (roll {roll}/{highestDetectChance})");
            }
        }
    }
}
