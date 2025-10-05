using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum EnemyState
{
    Patrol,    
    Alert,     
    Chase      
}

public class EnemyUnit : Unit
{
    public int visionRange = 2;
    public int DetectionChance => data.detectionChance;
    public EnemyState currentState = EnemyState.Patrol;
    public HeroUnit detectedHero;  

    private readonly HashSet<HeroUnit> alreadyAttacked = new();
    private GameObject highlightOverlay;
    private GameObject arrowInstance;
    private Canvas canvas;
    private Collider2D col;

    // ============================ INIT ============================
    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>(true);
        col = GetComponent<Collider2D>();
    }

    // ============================ STATE MACHINE ============================
    public void SetState(EnemyState newState, HeroUnit target = null)
    {
        currentState = newState;
        detectedHero = target;

        switch (currentState)
        {
            case EnemyState.Patrol:
                Debug.Log($"🟢 {name} quay lại tuần tra.");
                break;

            case EnemyState.Alert:
                Debug.Log($"🔴 {name} phát hiện {detectedHero?.name}! Báo động toàn bộ enemy!");
                EnemySystem.Instance.TriggerAlert(this);
                break;

            case EnemyState.Chase:
                Debug.Log($"⚡ {name} bắt đầu truy đuổi {detectedHero?.name}!");
                break;
        }
    }

    public bool HasTarget() => detectedHero != null && !detectedHero.IsDead;

    // ============================ TILE ENTRY ============================
    public override void OnEnterTile(Tile tile)
    {
        base.OnEnterTile(tile);
        if (tile == null) return;

        SetVisibility(tile.IsVisible);

        foreach (var unit in tile.occupyingUnits)
        {
            if (unit is HeroUnit hero && !hero.IsDead && !alreadyAttacked.Contains(hero))
            {
                OnHeroDetected(hero);
            }
        }
    }

    private void OnHeroDetected(HeroUnit hero)
    {
        alreadyAttacked.Add(hero);
        hero.IsDetected = true;
        HeroAlertUI.Instance?.SetDetected(true);

        Debug.Log($"🚨 {hero.name} bị phát hiện bởi {name}");
        SetState(EnemyState.Alert, hero);

        // Gọi ActionSystem xử lý tấn công
        TryAttackHero(hero);
    }

    // ============================ COMBAT ============================
    public bool CanAttack(HeroUnit hero)
    {
        if (hero == null || hero.IsDead) return false;
        int distance = GridManager.Instance.GetDistance(currentPosition, hero.currentPosition);
        return distance <= AttackRange; // dùng attackRange từ UnitData
    }

    public void TryAttackHero(HeroUnit hero)
    {
        if (!CanAttack(hero)) return;
        Debug.Log($"⚔️ {name} tấn công {hero.name}");
        ActionSystem.Instance.AddReaction(new AttackHeroGA(this, hero));
    }

    // ============================ VISIBILITY ============================
    public void SetVisibility(bool visible)
    {
        if (canvas != null) canvas.enabled = visible;
        if (col != null) col.enabled = visible;
    }

    // ============================ UI INTERACTION ============================
    private void OnMouseDown()
    {
        if (TargetingSystem.Instance?.IsTargeting == true)
        {
            TargetingSystem.Instance.TrySelectEnemy(this);
            return;
        }

        if (SelectedHero == null) return;

        SkillBarUI skillBar = FindObjectOfType<SkillBarUI>();
        if (skillBar == null) return;

        if (SelectedEnemy == this)
        {
            OnDeselect();
            skillBar.Hide();
            skillBar.Setup(SelectedHero, SelectedHero.GetSkills(), null);
            skillBar.GetComponent<WorldSpaceUIFollow>().target = SelectedHero.transform;
            skillBar.Show();
        }
        else
        {
            SelectedEnemy = this;
            SetHighlight(true);
            skillBar.Setup(SelectedHero, GetInteractionSkills(), this);
            skillBar.GetComponent<WorldSpaceUIFollow>().target = transform;
            skillBar.Show();
        }
    }

    public override void OnDeselect()
    {
        if (SelectedEnemy == this)
            SelectedEnemy = null;

        SetHighlight(false);
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
        EnemySystem.Instance.CheckAlertEnd();
    }

    private void DropLoot()
    {
        Tile tile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (tile == null || Random.value > 0.6f) return;

        GameObject lootPrefab = Resources.Load<GameObject>("Prefabs/ItemLoot");
        if (lootPrefab == null) return;

        Vector3 basePos = GridManager.Instance.GetWorldPosition(currentPosition);
        Vector3 offset = tile.GetLocalOffsetForUnit(this);
        Vector3 spawnPos = basePos + offset + new Vector3(0, 0, -0.5f);

        GameObject lootObj = Instantiate(lootPrefab, spawnPos, Quaternion.identity, tile.transform);
        SpriteRenderer sr = lootObj.GetComponent<SpriteRenderer>();
        SpriteRenderer tileSr = tile.GetComponent<SpriteRenderer>();
        if (sr != null && tileSr != null)
            sr.sortingOrder = tileSr.sortingOrder + 1;

        LootItem loot = lootObj.GetComponent<LootItem>();
        loot?.Init(null, 1, tile);
        tile.AddLoot(loot);
    }

    // ============================ SKILLS ============================
    private List<SkillData> GetInteractionSkills()
    {
        return new List<SkillData>
        {
            Resources.Load<SkillData>("Skills/AssassinateSkill"),
            Resources.Load<SkillData>("Skills/FightSkill")
        };
    }
}
