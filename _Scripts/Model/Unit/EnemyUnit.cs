// File: EnemyUnit.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyState { Patrol, Chase, LostTrack }

public class EnemyUnit : Unit
{
    [Header("Config")]
    public int visionRange = 0;
    public EnemyState currentState = EnemyState.Patrol;
    public HeroUnit detectedHero;
    public int DetectionChance => data.detectionChance;

    [Header("Tracking")]
    public List<Vector2Int> heroVisibleHistory = new();
    private bool IsTrackingState => currentState == EnemyState.Chase || currentState == EnemyState.LostTrack;


    [Header("UI / Marker")]
    private GameObject highlightOverlay;
    private GameObject arrowInstance;

    // GIỮ lại alreadyAttacked nếu logic Attack Phase có dùng để tránh tấn công lặp
    private readonly HashSet<HeroUnit> alreadyAttacked = new(); 
    private Canvas canvas;
    private Collider2D col;

    // ============================ INIT ============================
    public override void Setup(UnitData data)
    {
        base.Setup(data);
        EnemySystem.Instance?.RegisterEnemy(this);
    }

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>(true);
        col = GetComponent<Collider2D>();
    }

    // THÊM HÀM NÀY: Dùng để reset trạng thái Enemy vào đầu mỗi lượt Enemy
    public void ResetTurnState()
    {
        alreadyAttacked.Clear();
    }

    // ============================ STATE MACHINE ============================
    public void SetState(EnemyState newState, HeroUnit target = null)
    {
        if (currentState == newState) return;

        switch (newState)
        {
            case EnemyState.Chase:
                if (currentState == EnemyState.Patrol) visionRange += 2;
                if (currentState == EnemyState.LostTrack)
                {
                    heroVisibleHistory.Clear();
                    if (target != null) heroVisibleHistory.Add(target.currentPosition);
                }
                break;

            case EnemyState.LostTrack:
                break;

            case EnemyState.Patrol:
                heroVisibleHistory.Clear();
                if (currentState != EnemyState.Patrol)
                    visionRange = Mathf.Max(0, visionRange - 2);
                detectedHero = null;
                break;
        }

        currentState = newState;
        detectedHero = target;
        EnemySystem.Instance?.UpdateEnemyStateUI();
    }

    // ============================ TILE ENTRY ============================
    public override void OnEnterTile(Tile tile)
    {
        base.OnEnterTile(tile);
        if (tile == null) return;

        SetVisibility(tile.IsVisible);

        foreach (var unit in tile.occupyingUnits)
        {
            if (unit is HeroUnit hero && !hero.IsDead )
            {
                if (hero.IsDetected)
                {
                    if (!alreadyAttacked.Contains(hero))
                    {
                        OnHeroDetected(hero);
                    }
                }
            }
        }
    }

    // GIỮ LẠI hàm này cho logic cũ (ví dụ: nếu có logic khác gọi)
    private void OnHeroDetected(HeroUnit hero)
    {
        alreadyAttacked.Add(hero);
        hero.IsDetected = true;
        HeroAlertUI.Instance?.SetDetected(true);
        // BỎ QUA TryAttackHero(hero) để Attack Phase của Enemy Turn xử lý
    }

    // ============================ COMBAT ============================
    public bool CanAttack(HeroUnit hero)
    {
        if (hero == null || hero.IsDead) return false;
        int distance = GridManager.Instance.GetDistance(currentPosition, hero.currentPosition);
        return distance <= AttackRange;
    }

    public void TryAttackHero(HeroUnit hero)
    {
        if (!CanAttack(hero)) return;
        ActionSystem.Instance.AddReaction(new AttackHeroGA(this, hero));
    }

    // ============================ VISIBILITY ============================
    public void SetVisibility(bool visible)
    {
        if (canvas != null) canvas.enabled = visible;
        if (col != null) col.enabled = visible;
    }

    public void EvaluateVisionForEnemy()
    {
        if (!IsTrackingState) return;
        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero.IsDead) continue;
            if (VisionSystem.Instance.IsTileInVision(this, hero.currentPosition))
                heroVisibleHistory.AddIfNotContains(hero.currentPosition);
        }
    }

    // ============================ HERO MOVEMENT TRACKING ============================
    public void RegisterHeroMovementListener()
    {
        HeroUnit.OnHeroMoved -= OnHeroMovedHandler; // prevent duplicate registration
        HeroUnit.OnHeroMoved += OnHeroMovedHandler;
    }

    private void OnHeroMovedHandler(HeroUnit hero, Vector2Int pos)
    {
        if (IsDead || hero == null || hero.IsDead) return;
        if (IsTrackingState || hero.IsDetected)
        {
            if (VisionSystem.Instance.IsTileInVision(this, pos))
                heroVisibleHistory.AddIfNotContains(pos);
        }
    }

    // ============================ UI INTERACTION ============================
    private void OnMouseDown()
    {
        HeroUnit hero = SelectedHero;
        
        // 1. Xử lý Targeting Mode (Giữ nguyên)
        if (TargetingSystem.Instance?.IsTargeting == true)
        {
            TargetingSystem.Instance.TrySelectEnemy(this);
            return;
        }

        if (hero == null) return;
        
        if (SelectedEnemy == this)
        {
            OnDeselect(); 
            UIManager.Instance.ShowSkillBar(hero); 
        }
        else
        {
            if (SelectedEnemy != null && SelectedEnemy != this)
            {
                SelectedEnemy.OnDeselect();
            }
            
            // Chọn Enemy mới và Highlight
            SelectedEnemy = this;
            SetHighlight(true);
            UIManager.Instance.ShowSkillBarForTarget(hero, this, GetInteractionSkills());
        }
    }

    public override void OnDeselect()
    {
        if (SelectedEnemy == this) SelectedEnemy = null;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideSkillBar();
        }
        SetHighlight(false);

        if (arrowInstance != null) 
        {
            if (arrowInstance.gameObject != null) 
            {
                Destroy(arrowInstance.gameObject);
            }
            arrowInstance = null;
        } 
        
        if (highlightOverlay != null) 
        {
            if (highlightOverlay.gameObject != null)
            {
                Destroy(highlightOverlay.gameObject);
            }
            highlightOverlay = null;
        }
    }

    private void OnDestroy() => OnDeselect();

    // ============================ HIGHLIGHT UI ============================
    public void SetHighlight(bool active)
    {
        if (highlightOverlay == null)
        {
            highlightOverlay = new GameObject("HighlightOverlay");
            highlightOverlay.transform.SetParent(icon.transform);
            highlightOverlay.transform.SetSiblingIndex(icon.transform.GetSiblingIndex() + 1);
            highlightOverlay.transform.localPosition = Vector3.zero;
            highlightOverlay.transform.localScale = Vector3.one;

            Image overlayImage = highlightOverlay.AddComponent<Image>();
            overlayImage.sprite = icon.sprite;
            overlayImage.color = new Color(1f, 0f, 0f, 0.5f);
            overlayImage.raycastTarget = false;
        }
        highlightOverlay.SetActive(active);

        if (active && arrowInstance == null)
        {
            arrowInstance = Instantiate(Resources.Load<GameObject>("Prefabs/ArrowUI"));
            arrowInstance.transform.Find("ArrowContainer").GetComponent<ArrowFollowUnit>().target = transform;
        }

        if (arrowInstance != null)
            arrowInstance.SetActive(active);
    }

    // ============================ DEATH & LOOT ============================
    public override void Die()
    {
        DropLoot();
        base.Die();
        EnemySystem.Instance?.UnregisterEnemy(this);
    }

    private void DropLoot()
    {
        Tile tile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (tile == null || Random.value > 0.6f) return;

        GameObject lootPrefab = Resources.Load<GameObject>("Prefabs/ItemLoot");
        if (lootPrefab == null) return;

        Vector3 spawnPos = GridManager.Instance.GetWorldPosition(currentPosition) + tile.GetLocalOffsetForUnit(this) + new Vector3(0, 0, -0.5f);

        GameObject lootObj = Instantiate(lootPrefab, spawnPos, Quaternion.identity, tile.transform);
        SpriteRenderer sr = lootObj.GetComponent<SpriteRenderer>();
        SpriteRenderer tileSr = tile.GetComponent<SpriteRenderer>();
        if (sr != null && tileSr != null) sr.sortingOrder = tileSr.sortingOrder + 1;

        LootItem loot = lootObj.GetComponent<LootItem>();
        loot?.Init(null, 1, tile);
        tile.AddLoot(loot);
    }

    // ============================ SKILLS ============================
    private List<SkillData> GetInteractionSkills()
    {
        return new List<SkillData>
        {
            Resources.Load<SkillData>("Skills/Skill Name/Fight"),
            Resources.Load<SkillData>("Skills/Skill Name/Assassinate")      
        };
    }
}