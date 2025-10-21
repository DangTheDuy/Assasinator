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
        HeroUnit hero = SelectedHero ;
        if (TargetingSystem.Instance?.IsTargeting == true)
        {
            TargetingSystem.Instance.TrySelectEnemy(this);
            return;
        }

        if (hero == null) return;
        
        // 3. Xử lý logic chọn/bỏ chọn
        if (SelectedEnemy == this)
        {
            // 3a. Nếu đã chọn Enemy này: Bỏ chọn
            OnDeselect();
            
            // 🚨 SỬA LỖI: Ẩn SkillBar hiện tại (của Enemy)
            UIManager.Instance.HideSkillBar(); 

            // 🚨 THÊM TÍNH NĂNG: Hiển thị lại Skill Bar mặc định của Hero
            // (Nếu bạn muốn Skill Bar của Hero hiện lên sau khi bỏ chọn Enemy)
            // UIManager.Instance.ShowSkillBar(hero); // Cần hàm ShowSkillBar đơn giản
        }
        else
        {
            // 3b. Nếu chưa chọn hoặc chọn Enemy khác: Chọn Enemy mới
            
            // Bỏ chọn Enemy cũ trước (nếu có)
            if (Unit.SelectedEnemy != null && Unit.SelectedEnemy != this)
            {
                Unit.SelectedEnemy.OnDeselect(); // Gọi OnDeselect của Enemy cũ
            }
            
            // Chọn Enemy mới và Highlight
            Unit.SelectedEnemy = this;
            SetHighlight(true);
            
            // 🚨 GỌI HÀM CỦA UIManager: Hiển thị Skill Bar Tương Tác
            // Đây là bước quan trọng nhất để hiện Skill Bar trên đầu Enemy
            UIManager.Instance.ShowSkillBarForTarget(hero, this, GetInteractionSkills());
        }
    }

    // Giữ nguyên OnDeselect, nhưng đảm bảo nó gọi HideSkillBar nếu cần
    public override void OnDeselect()
    {
        // ... (Giữ nguyên logic SelectedEnemy = null, SetHighlight, Destroy arrow/overlay) ...
        if (Unit.SelectedEnemy == this) Unit.SelectedEnemy = null;
        SetHighlight(false);

        // 🚨 QUAN TRỌNG: Ẩn Skill Bar khi mục tiêu bị bỏ chọn
        UIManager.Instance.HideSkillBar();
        
        if (arrowInstance != null) Destroy(arrowInstance);
        if (highlightOverlay != null) Destroy(highlightOverlay);
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