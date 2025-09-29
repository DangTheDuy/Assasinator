
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
    public int DetectionChance => data.detectionChance;
    private GameObject highlightOverlay;
    private GameObject arrowInstance;
    private HashSet<HeroUnit> alreadyAttacked = new HashSet<HeroUnit>();

    // ================================= ON MOUSE DOWN ================================================
    private void OnMouseDown()
    {
        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsTargeting)
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
            return;
        }

        if (SelectedEnemy == null)
        {
            SelectedEnemy = this;
            SetHighlight(true);

            List<SkillData> interactionSkills = GetInteractionSkills();
            skillBar.Setup(SelectedHero, interactionSkills, this);
            skillBar.GetComponent<WorldSpaceUIFollow>().target = transform;
            skillBar.Show();
        }
    }

    // =========================================== ON DESELECT =============================================
    public override void OnDeselect()
    {
        if (SelectedEnemy == this)
            SelectedEnemy = null;

        SetHighlight(false);

        if (arrowInstance != null)
        {
            Destroy(arrowInstance);
            arrowInstance = null;
        }

        if (highlightOverlay != null)
        {
            Destroy(highlightOverlay);
            highlightOverlay = null;
        }
    }

    // ========================================= ON DESTROY =================================================
    private void OnDestroy()
    {
        OnDeselect();
    }

    public override void Die()
    {
        Tile tile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (tile != null)
        {
            float dropChance = 0.6f; // 60% cơ hội rơi loot
            if (Random.value <= dropChance)
            {
                GameObject lootPrefab = Resources.Load<GameObject>("Prefabs/ItemLoot");
                if (lootPrefab != null)
                {
                    // 🔹 Lấy vị trí world chính xác theo slot (giống như khi đặt enemy)
                    Vector3 basePos = GridManager.Instance.GetWorldPosition(currentPosition);
                    Vector3 offset = tile.GetLocalOffsetForUnit(this);
                    Vector3 spawnPos = new Vector3(basePos.x + offset.x, basePos.y + offset.y, -0.5f);

                    GameObject lootObj = Instantiate(lootPrefab, spawnPos, Quaternion.identity, tile.transform);

                    // Sửa sorting order để hiển thị trên tile
                    SpriteRenderer sr = lootObj.GetComponent<SpriteRenderer>();
                    SpriteRenderer tileSr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null && tileSr != null)
                    {
                        sr.sortingOrder = tileSr.sortingOrder + 1;
                    }

                    LootItem loot = lootObj.GetComponent<LootItem>();
                    if (loot != null)
                    {
                        // Spawn cái thùng, itemData random khi nhặt
                        loot.Init(null, 1, tile);
                        tile.AddLoot(loot);
                    }
                }
            }
            else
            {
                Debug.Log("❌ Enemy không rơi loot gì.");
            }
        }

        base.Die();
    }
// ===================================== ENTER TILE ============================================
    public override void OnEnterTile(Tile tile)
    {
        base.OnEnterTile(tile);
        if (tile == null) return;

        foreach (var unit in tile.occupyingUnits)
        {
            if (unit is HeroUnit hero && !hero.IsDead)
            {
                if (alreadyAttacked.Contains(hero)) continue; // tránh đánh lặp
                alreadyAttacked.Add(hero);

                hero.IsDetected = true;
                Debug.Log($"🚨 {hero.name} bị phát hiện bởi {name} (enemy bước vào tile)");
                HeroAlertUI.Instance?.SetDetected(true);

                ActionSystem.Instance.AddReaction(new AttackHeroGA(this, hero));
            }
        }
    }

    // ========================================= SET HIGHLIGHT =============================================
    public void SetHighlight(bool active)
    {
        if (this == null || gameObject == null)
        {
            return;
        }

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

        if (active)
        {
            if (arrowInstance == null)
            {
                arrowInstance = Instantiate(Resources.Load<GameObject>("Prefabs/ArrowUI"));
                arrowInstance.transform.Find("ArrowContainer").GetComponent<ArrowFollowUnit>().target = transform;
            }
            arrowInstance.SetActive(true);
        }
        else
        {
            if (arrowInstance != null)
                arrowInstance.SetActive(false);
        }
    }

    // ============================================== GET SKILL ==========================================
    private List<SkillData> GetInteractionSkills()
    {
        return new List<SkillData>
        {
            Resources.Load<SkillData>("Skills/AssassinateSkill"),
            Resources.Load<SkillData>("Skills/FightSkill")
        };
    }

}

