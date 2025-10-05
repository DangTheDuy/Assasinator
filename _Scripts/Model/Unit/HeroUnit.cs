using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[System.Serializable]
public class HeroUnit : Unit
{
    // ====================================== FIELDS ======================================
    [Header("Hero Settings")]
    public List<SkillData> skills = new List<SkillData>();

    [Header("Inventory")]
    public List<ItemStack> inventory = new List<ItemStack>();

    [Header("UI/Arrow Settings")]
    public GameObject arrow;
    public GameObject apPrefab;
    public GameObject emptyApPrefab;
    public Transform apContainer;

    public bool IsDetected { get; set; }
    public static System.Action<HeroUnit> OnHeroSpawned;
    public int visionRange = 2;

    public int currentAP;
    public int CurrentHP => GetCurrentHealth();

    private GameObject arrowInstance;

    // ================================= WATER SYSTEM ====================================
    [Header("Water & Drowning")]
    public bool canWalkOnWater = false;     // buff cho phép đi trên nước
    public bool isDrowning = false;         // đang đuối nước
    public int drownTurnsLeft = 0;          // số lượt còn lại trước khi chết

    // ==================================== SETUP ========================================
    public override void Setup(UnitData data)
    {
        base.Setup(data);
        VisionSystem.Instance.UpdateDiamondVision(currentPosition, visionRange, null, -1, this);

        // kỹ năng
        skills.Clear();
        if (data.skills != null && data.skills.Count > 0)
            skills.AddRange(data.skills);
        else
            Debug.LogWarning($"{data.unitName} chưa có skill nào trong UnitData!");

        // AP & HUD
        currentAP = data.maxAP;
        InitAPBar();
        UpdateAP(currentAP);

        HeroHUDManager hudManager = FindObjectOfType<HeroHUDManager>();
        if (hudManager != null) hudManager.CreateHUD(this);

        OnHeroSpawned?.Invoke(this);
    }

    // ================================= SELECT / DESELECT ================================
    public override void OnSelect()
    {
        base.OnSelect();

        if (SelectedEnemy != null)
        {
            SelectedEnemy.SetHighlight(false);
            SelectedEnemy = null;
        }

        if (SelectedHero != null && SelectedHero != this)
            SelectedHero.OnDeselect();

        SelectedHero = this;
        UIManager.Instance.ShowSkillBar(this);
        ShowArrow();
        HighlightMovementTiles();
    }

    public override void OnDeselect()
    {
        base.OnDeselect();

        if (SelectedHero == this)
            SelectedHero = null;

        UIManager.Instance.HideSkillBar();
        HideArrow();
        ClearTileHighlights();

        if (SelectedEnemy != null)
        {
            SelectedEnemy.SetHighlight(false);
            SelectedEnemy = null;
        }
    }

    // ===================================== MOVE =========================================
    public override void MoveTo(Vector3 worldPos, Vector2Int gridPos)
    {
        if (IsDetected || isDrowning) return;

        Tile targetTile = GridManager.Instance.GetTileAtPosition(gridPos);
        if (targetTile == null) return;

        // chặn đi vào nước nếu không có buff
        if (targetTile is WaterTile && !canWalkOnWater)
        {
            Debug.Log($"{data.unitName} không thể đi vào ô nước!");
            return;
        }

        int cost = targetTile.MovementCost;
        if (!HasEnoughAP(cost)) return;

        Vector2Int prev = currentPosition;
        int prevRange = visionRange;

        base.MoveTo(worldPos, gridPos);
        SpendAP(cost);
        VisionSystem.Instance.UpdateDiamondVision(gridPos, visionRange, prev, prevRange, this);
    }

    public override void OnEnterTile(Tile tile)
    {
        base.OnEnterTile(tile);
        if (tile == null) return;

        // check enemy detect
        bool hasEnemy = tile.occupyingUnits.Exists(u => u is EnemyUnit);
        if (hasEnemy) tile.CheckDetection();

        // check nước
        if (tile is WaterTile && !canWalkOnWater)
            StartDrowning();
    }

    // ================================== DROWNING LOGIC ==================================
    public void EnableWaterWalk(int turns)
    {
        canWalkOnWater = true;
        StartCoroutine(WaterWalkBuff(turns));
    }

    private IEnumerator WaterWalkBuff(int turns)
    {
        int startTurn = TurnManager.Instance.CurrentTurn;
        yield return new WaitUntil(() => 
            TurnManager.Instance.CurrentTurn >= startTurn + turns );
        EndWaterWalkEffect();
    }

    public void EndWaterWalkEffect()
    {
        canWalkOnWater = false;
        Tile tile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (tile is WaterTile)
            StartDrowning();
    }

    public void StartDrowning()
    {
        if (isDrowning) return;
        isDrowning = true;
        drownTurnsLeft = 3;
        currentAP = 0;
        UpdateAP(currentAP);
        UpdateHUD();
        GetComponentInChildren<Image>().color = new Color(0.5f, 0.6f, 1f, 0.9f);
        Debug.Log($"{data.unitName} bị đuối nước!");
    }

    public void OnTurnStart()
    {
        if (!isDrowning) return;
        drownTurnsLeft--;
        Debug.Log($"{data.unitName} đang đuối nước! Còn {drownTurnsLeft} lượt!");
        if (drownTurnsLeft <= 0)
            Die();
    }

    public bool TryRescue(HeroUnit target)
    {
        if (target == null || !target.isDrowning) return false;

        int dist = GridManager.Instance.GetDistance(currentPosition, target.currentPosition);
        if (dist > 1)
        {
            Debug.Log($"{data.unitName} quá xa để cứu {target.data.unitName}!");
            return false;
        }

        target.isDrowning = false;
        target.drownTurnsLeft = 0;
        target.GetComponent<SpriteRenderer>().color = Color.white;

        // kéo lên cùng ô
        target.currentPosition = currentPosition;
        target.transform.position = transform.position + new Vector3(0, 0.2f, 0);
        GridManager.Instance.GetTileAtPosition(currentPosition).SetOccupied(target);

        SpendAP(2);
        Debug.Log($"{data.unitName} đã cứu {target.data.unitName} khỏi đuối!");
        return true;
    }
    // =================== DIE ================================
    public override void Die()
    {
        if (VisionSystem.Instance != null)
            VisionSystem.Instance.RemoveHeroVision(this);
        HideArrow();
        base.Die();
    }

    // ================================== AP SYSTEM =======================================
    private void InitAPBar()
    {
        if (apPrefab == null || apContainer == null)
        {
            Debug.LogWarning("Thiếu prefab hoặc container AP!");
            return;
        }

        foreach (Transform child in apContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < data.maxAP; i++)
            Instantiate(apPrefab, apContainer);
    }

    public void UpdateAP(int value)
    {
        if (apContainer == null) return;
        int maxAP = data.maxAP;

        foreach (Transform child in apContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < maxAP; i++)
        {
            GameObject icon = i < value ? apPrefab : emptyApPrefab;
            Instantiate(icon, apContainer);
        }

        if (SelectedHero != null)
            UIManager.Instance.ShowSkillBar(this);
    }

    public bool HasEnoughAP(int amount) => currentAP >= amount;

    public void SpendAP(int amount)
    {
        currentAP = Mathf.Max(0, currentAP - amount);
        UpdateAP(currentAP);
        UpdateHUD();
    }

    public void RefillAP()
    {
        currentAP = data.maxAP;
        UpdateAP(currentAP);
        UpdateHUD();
    }

    // ================================= INVENTORY =======================================
    public void UseItem(ItemStack stack)
    {
        if (stack == null || stack.itemData == null) return;

        ItemData item = stack.itemData;
        switch (item.type)
        {
            case ItemType.Heal:
                if (!stack.Consume()) return;
                currentHealth = Mathf.Min(data.maxHealth, currentHealth + item.value);
                break;

            case ItemType.RestoreAP:
                if (!stack.Consume()) return;
                currentAP = Mathf.Min(data.maxAP, currentAP + item.value);
                break;

            case ItemType.Shuriken:
                if (item.linkedSkill != null)
                    TargetingSystem.Instance.EnterTargetMode(this, item.linkedSkill, stack);
                return;
            case ItemType.Water:
            if (!stack.Consume()) return;
            Debug.Log($"{name} dùng {item.itemName} → có thể đi trên nước trong 1 lượt!");
            EnableWaterWalk(1);
            break;

        }

        if (stack.quantity <= 0) inventory.Remove(stack);
        Debug.Log($"{name} dùng {item.itemName}, còn lại {stack.quantity}");
        UpdateHUD();
    }

    public void AddItem(ItemData data, int amount = 1)
    {
        ItemStack existing = inventory.Find(s => s.itemData == data);
        if (existing != null) existing.Add(amount);
        else inventory.Add(new ItemStack(data, amount));

        HeroHUDManager hud = FindObjectOfType<HeroHUDManager>();
        hud?.UpdateHeroItems(this, inventory);
    }

    // ================================= ARROW ===========================================
    private void ShowArrow()
    {
        if (arrowInstance == null)
        {
            arrowInstance = Instantiate(Resources.Load<GameObject>("Prefabs/ArrowUI"));
            var follow = arrowInstance.GetComponentInChildren<ArrowFollowUnit>();
            if (follow != null) follow.target = transform;
        }
        arrowInstance.SetActive(true);
    }

    private void HideArrow()
    {
        if (arrowInstance != null)
            arrowInstance.SetActive(false);
    }

    // =============================== TILE HIGHLIGHT =====================================
    private void HighlightMovementTiles()
    {
        foreach (var kv in GridManager.Instance.tiles)
        {
            int distance = GridManager.Instance.GetDistance(currentPosition, kv.Key);
            if (distance <= data.moveRange && GridManager.Instance.IsCellAvailableForMovement(kv.Key))
                kv.Value.Highlight(true);
        }
    }

    private void ClearTileHighlights()
    {
        foreach (var kv in GridManager.Instance.tiles)
            kv.Value.Highlight(false);
    }

    // =============================== HUD UPDATE ========================================
    public void UpdateHUD()
    {
        var hud = FindObjectOfType<HeroHUDManager>();
        if (hud == null) return;

        hud.UpdateHeroHP(this, CurrentHP, data.maxHealth);
        hud.UpdateHeroAP(this, currentAP, data.maxAP);
        hud.UpdateHeroItems(this, inventory);
    }

    // =============================== ACCESS ============================================
    public List<SkillData> GetSkills() => skills;
}
