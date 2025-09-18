using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileData tileData;
    public Vector2Int gridPosition;
    public int MaxUnitsPerTile => 8;
    public virtual bool IsObstacle { get; protected set; } = false;
    public virtual int MovementCost => 1; // mặc định 1 AP
    public virtual float DetectionModifier => 1f; // không giảm phát hiện
    public virtual bool CanHide => false; 
    private bool detectionCheckedThisFrame = false;
    private GameObject overlay;
    public List<Unit> occupyingUnits = new List<Unit>();
    private Dictionary<Unit, int> heroSlots = new Dictionary<Unit, int>();
    private Dictionary<Unit, int> enemySlots = new Dictionary<Unit, int>();

    private static readonly int[] HeroSlotPool = { 7, 8, 9, 4 };
    private static readonly int[] EnemySlotPool = { 1, 2, 3, 6 };

// ======================================== INIT ============================================= 
    public void Init(int x, int y, TileData data)
    {
        gridPosition = new Vector2Int(x, y);
        tileData = data;

        overlay = new GameObject("Overlay");
        overlay.transform.SetParent(transform);
        overlay.transform.localPosition = Vector3.zero;
        overlay.transform.localScale = Vector3.one;

        SpriteRenderer overlaySr = overlay.AddComponent<SpriteRenderer>();
        overlaySr.sprite = GetComponent<SpriteRenderer>().sprite;
        overlaySr.color = new Color(0f, 1f, 1f, 0.3f);
        overlaySr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

        overlay.SetActive(false);
    }

    void LateUpdate()
    {
        detectionCheckedThisFrame = false;
    }

// ======================================== ON MOUSE DOWN ======================================== 
    private void OnMouseDown()
    {
        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsTargeting) return;
        if (SkillBarUI.IsEnemyInteractionOpen) return;

        Unit selected = Unit.GetSelectedUnit();
        if (selected == null || selected.currentPosition == gridPosition) return;

        if (!IsObstacle && occupyingUnits.Count < MaxUnitsPerTile)
        {
            int distance = GridManager.Instance.GetDistance(selected.currentPosition, gridPosition);
            if (distance <= selected.data.moveRange)
            {
                Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);
                selected.MoveTo(basePos, gridPosition);
            }
            else
            {
                Debug.Log($"{selected.name} không thể đi xa hơn {selected.data.moveRange} ô!");
            }
        }
    }

// ======================================== SET/ UNOCCUPIED ======================================== 
    public void SetOccupied(Unit unit)
    {
        if (unit == null) return;

        bool isHero = unit is HeroUnit;
        int sameTypeCount = occupyingUnits.FindAll(u => isHero ? u is HeroUnit : u is EnemyUnit).Count;

        if (sameTypeCount >= 4)
        {
            Debug.LogWarning($"Tile  đã đủ Không thể thêm {unit.name}");
            return;
        }

        if (!occupyingUnits.Contains(unit))
            occupyingUnits.Add(unit);

        int slot = FindAvailableSlot(isHero);
        if (slot == -1) return;

        if (isHero)
            heroSlots[unit] = slot;
        else
            enemySlots[unit] = slot;

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
        occupyingUnits.RemoveAll(u => u == unit || u == null);
        heroSlots.Remove(unit);
        enemySlots.Remove(unit);
    }
// ======================================== LOCAL OFFSET ========================================
    public Vector3 GetLocalOffsetForUnit(Unit unit)
    {
        if (unit == null) return Vector3.zero;
        int index = -1;
        bool isHero = unit is HeroUnit;
        if (isHero && heroSlots.ContainsKey(unit))
            index = heroSlots[unit];
        else if (!isHero && enemySlots.ContainsKey(unit))
            index = enemySlots[unit];

        if (index == -1 || index == 5) return Vector3.zero;
        float baseSize = GetComponent<SpriteRenderer>().bounds.size.x;
        float spacing = baseSize * 0.3f; // 35% chiều rộng tile

        int row = (index - 1) / 3;
        int col = (index - 1) % 3;
        float x = (col - 1) * spacing;
        float y = (1 - row) * spacing;
        return new Vector3(x, y, 0);
    }

    private int FindAvailableSlot(bool isHero)
    {
        int[] pool = isHero ? HeroSlotPool : EnemySlotPool;
        var dict = isHero ? heroSlots : enemySlots;

        foreach (int slot in pool)
        {
            if (!dict.ContainsValue(slot))
                return slot;
        }
        return -1;
    }
// =========================================== CHECK DETECT ============================================
    public void CheckDetection()
        {
            int highestDetectChance = 0;
            foreach (var unit in occupyingUnits)
            {
                if (unit is EnemyUnit enemy)
                    highestDetectChance = Mathf.Max(highestDetectChance, enemy.DetectionChance);
            }

            if (highestDetectChance > 0)
            {
                int roll = Random.Range(0, 100);
                if (roll < highestDetectChance)
                {
                    Debug.Log($"Hero bị phát hiện! (roll {roll}/{highestDetectChance})");
                    foreach (var unit in occupyingUnits)
                    {
                        if (unit is HeroUnit hero)
                            hero.IsDetected = true;
                    }
                }
                else
                {
                    Debug.Log($"Hero chưa bị phát hiện (roll {roll}/{highestDetectChance})");
                }
            }
        }
// =============================================== HELPER ===========================================
    public void Highlight(bool active)
    {
        if (overlay != null)
            overlay.SetActive(active);
    }

    public bool CanAccept(Unit unit)
    {
        if (unit is EnemyUnit)
            return occupyingUnits.FindAll(u => u is EnemyUnit).Count < 4;
        if (unit is HeroUnit)
            return occupyingUnits.FindAll(u => u is HeroUnit).Count < 4;
        return true;
    }
    
    public void PlaceUnit(Unit unit)
    {
        if (unit == null) return;

        SetOccupied(unit);
        Vector3 offset = GetLocalOffsetForUnit(unit);
        Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);

        unit.transform.position = new Vector3(basePos.x + offset.x, basePos.y + offset.y, -1f);
        unit.currentPosition = gridPosition;
    }

}
