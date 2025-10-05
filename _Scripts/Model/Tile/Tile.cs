using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // ================== DATA & STATE ==================
    public TileData tileData;
    public Vector2Int gridPosition;
    public List<Unit> occupyingUnits = new List<Unit>();
    public List<LootItem> lootItems = new List<LootItem>();

    public int MaxUnitsPerTile => 8;
    public virtual bool IsObstacle { get; protected set; } = false;
    public virtual int MovementCost => 1;
    public virtual float DetectionModifier => 1f;
    public virtual bool CanHide => false;

    // Vision
    public bool IsVisible { get; private set; } = false;
    public bool IsSeen { get; private set; } = false;
    public int visibleCount = 0;

    // UI
    private GameObject overlay;
    private TextMeshPro detectText;
    private SpriteRenderer fogRenderer;

    // Unit slots
    private Dictionary<Unit, int> heroSlots = new Dictionary<Unit, int>();
    private Dictionary<Unit, int> enemySlots = new Dictionary<Unit, int>();
    private static readonly int[] HeroSlotPool = { 7, 8, 9, 4 };
    private static readonly int[] EnemySlotPool = { 1, 2, 3, 6 };

    // Prefabs
    public GameObject radarPrefab;

    // ================== INIT ==================
    public void Init(int x, int y, TileData data)
    {
        gridPosition = new Vector2Int(x, y);
        tileData = data;

        CreateOverlay();
        CreateDetectText();
        CreateFog();

        IsVisible = false;
        IsSeen = false;
        visibleCount = 0;
        ApplyFog();
    }

    private void CreateOverlay()
    {
        overlay = new GameObject("Overlay");
        overlay.transform.SetParent(transform);
        overlay.transform.localPosition = Vector3.zero;
        overlay.transform.localScale = Vector3.one;

        SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.color = new Color(0f, 1f, 1f, 0.3f);
        sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

        overlay.SetActive(false);
    }

    private void CreateDetectText()
    {
        GameObject textObj = new GameObject("DetectChanceText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.2f);

        detectText = textObj.AddComponent<TextMeshPro>();
        detectText.alignment = TextAlignmentOptions.Center;
        detectText.fontSize = 5;
        detectText.color = Color.red;
        detectText.text = "";
        detectText.gameObject.SetActive(false);
    }

    private void CreateFog()
    {
        GameObject fogObj = new GameObject("FogOverlay");
        fogObj.transform.SetParent(transform);
        fogObj.transform.localPosition = Vector3.zero;
        fogObj.transform.localScale = Vector3.one;

        fogRenderer = fogObj.AddComponent<SpriteRenderer>();
        fogRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
        fogRenderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 5;
        fogRenderer.color = Color.black;
    }

    // ================== OCCUPANCY ==================
    public void SetOccupied(Unit unit)
    {
        if (unit == null) return;

        bool isHero = unit is HeroUnit;
        int sameTypeCount = occupyingUnits.FindAll(u => isHero ? u is HeroUnit : u is EnemyUnit).Count;
        if (sameTypeCount >= 4)
        {
            Debug.LogWarning($"Tile {gridPosition} đã đủ slot cho {(isHero ? "Hero" : "Enemy")}");
            return;
        }

        if (!occupyingUnits.Contains(unit))
            occupyingUnits.Add(unit);

        AssignSlot(unit, isHero);
        UpdateDetectDisplay();
        OnUnitEnter(unit);
    }

    public void SetUnoccupied(Unit unit)
    {
        if (unit == null) return;
        OnUnitExit(unit);

        occupyingUnits.RemoveAll(u => u == unit || u == null);
        heroSlots.Remove(unit);
        enemySlots.Remove(unit);
        UpdateDetectDisplay();
    }

    private void AssignSlot(Unit unit, bool isHero)
    {
        int[] pool = isHero ? HeroSlotPool : EnemySlotPool;
        var dict = isHero ? heroSlots : enemySlots;

        foreach (int slot in pool)
        {
            if (!dict.ContainsValue(slot))
            {
                dict[unit] = slot;
                return;
            }
        }
    }

    public Vector3 GetLocalOffsetForUnit(Unit unit)
    {
        if (unit == null) return Vector3.zero;
        int index = -1;
        if (unit is HeroUnit && heroSlots.TryGetValue(unit, out index)) { }
        else if (unit is EnemyUnit && enemySlots.TryGetValue(unit, out index)) { }

        if (index == -1 || index == 5) return Vector3.zero;

        float baseSize = GetComponent<SpriteRenderer>().bounds.size.x;
        float spacing = baseSize * 0.3f;

        int row = (index - 1) / 3;
        int col = (index - 1) % 3;
        float x = (col - 1) * spacing;
        float y = (1 - row) * spacing;
        return new Vector3(x, y, 0);
    }

    public void PlaceUnit(Unit unit)
    {
        if (unit == null) return;
        SetOccupied(unit);

        Vector3 offset = GetLocalOffsetForUnit(unit);
        Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPosition);
        Vector3 finalPos = new Vector3(basePos.x + offset.x, basePos.y + offset.y, -1f);

        unit.transform.position = finalPos;
        unit.currentPosition = gridPosition;
    }


    // ================== DETECTION ==================
    private int GetHighestDetectChance()
    {
        int highest = 0;
        foreach (var unit in occupyingUnits)
            if (unit is EnemyUnit enemy)
                highest = Mathf.Max(highest, enemy.DetectionChance);
        return highest;
    }

    public void CheckDetection()
    {
        int chance = GetHighestDetectChance();
        if (chance <= 0) return;

        Debug.Log($" Hero bước vào tile {gridPosition} có enemy, bắt đầu quét radar (chance {chance}%)");

        // Luôn phát radar hiệu ứng mỗi khi hero bước vào ô có enemy
        Vector3 spawnPos = GridManager.Instance.GetWorldPosition(gridPosition);
        GameObject radarObj = null;
        RadarEffect radar = null;

        if (radarPrefab != null)
        {
            radarObj = Instantiate(radarPrefab, spawnPos, Quaternion.identity);
            radar = radarObj.GetComponent<RadarEffect>();
        }

        // Callback sau khi radar chạy xong
        System.Action detectionResult = () =>
        {
            int roll = Random.Range(0, 100);
            bool detected = roll < chance;

            if (detected)
            {
                Debug.Log($"🚨 Hero bị phát hiện! (roll {roll}/{chance})");

                HeroUnit detectedHero = null;
                foreach (var unit in occupyingUnits)
                {
                    if (unit is HeroUnit hero && !hero.IsDead)
                    {
                        hero.IsDetected = true;
                        detectedHero = hero;
                        HeroAlertUI.Instance?.SetDetected(true);
                    }
                }

                if (detectedHero != null)
                {
                    foreach (var unit in occupyingUnits)
                    {
                        if (unit is EnemyUnit enemy && !enemy.IsDead)
                        {
                            enemy.SetState(EnemyState.Alert, detectedHero);
                            EnemySystem.Instance.TriggerAlert(enemy);
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"😶 Hero KHÔNG bị phát hiện (roll {roll}/{chance})");
            }
        };

        // Nếu có radar effect, chờ nó xong rồi roll phát hiện
        if (radar != null)
        {
            radar.onFinished = () => detectionResult.Invoke();
        }
        else
        {
            // Nếu không có radar effect prefab, roll luôn
            detectionResult.Invoke();
        }
    }

    public void UpdateDetectDisplay()
    {
        int chance = GetHighestDetectChance();
        detectText.text = chance > 0 ? $"{chance}%" : "";
        detectText.gameObject.SetActive(IsVisible && chance > 0);
    }

    // ================== LOOT ==================
    public void AddLoot(LootItem loot)
    {
        if (!lootItems.Contains(loot))
            lootItems.Add(loot);
    }

    public void RemoveLoot(LootItem loot) => lootItems.Remove(loot);

    // ================== VISION ==================
    public void AddVision()
    {
        visibleCount++;
        UpdateVision();
    }

    public void RemoveVision()
    {
        visibleCount = Mathf.Max(0, visibleCount - 1);
        UpdateVision();
    }

    private void UpdateVision()
    {
        bool newVisible = visibleCount > 0;
        if (newVisible != IsVisible)
        {
            IsVisible = newVisible;
            if (IsVisible) IsSeen = true;
        }

        ApplyFog();
        UpdateUnitVisibility(); 
    }


    public void ApplyFog()
    {
        if (fogRenderer == null) return;

        if (IsVisible)
        {
            fogRenderer.enabled = false;
        }
        else if (IsSeen)
        {
            fogRenderer.enabled = true;
            fogRenderer.color = new Color(0f, 0f, 0f, 0.5f);
        }
        else
        {
            fogRenderer.enabled = true;
            fogRenderer.color = Color.black;
        }
    }

    public void UpdateUnitVisibility()
    {
        foreach (var unit in occupyingUnits)
        {
            if (unit is EnemyUnit enemy)
                enemy.SetVisibility(IsVisible);
            else
                unit.gameObject.SetActive(IsVisible);
        }
        // Ẩn/hiện detectText theo tầm nhìn
        detectText.gameObject.SetActive(IsVisible && detectText.text != "");
    }

    // ================== HELPER ==================
    public void Highlight(bool active)
    {
        if (overlay != null) overlay.SetActive(active);
    }

    public bool CanAccept(Unit unit)
    {
        if (unit is EnemyUnit) return occupyingUnits.FindAll(u => u is EnemyUnit).Count < 4;
        if (unit is HeroUnit) return occupyingUnits.FindAll(u => u is HeroUnit).Count < 4;
        return true;
    }

    // ================== INPUT ==================
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
        }
    }
    public virtual void OnUnitEnter(Unit unit) { }
    public virtual void OnUnitExit(Unit unit) { }
}
