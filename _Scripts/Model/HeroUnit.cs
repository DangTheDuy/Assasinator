using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class HeroUnit : Unit
{
    [Header("Hero Settings")]
    public List<SkillData> skills = new List<SkillData>();
    public GameObject arrow;
    public GameObject apPrefab;
    public GameObject emptyApPrefab;
    public Transform apContainer;

    public bool IsDetected { get; set; }
    private GameObject arrowInstance;
    private int currentAP;

    // ============================================== SETUP ==============================================
    public override void Setup(UnitData data)
    {
        base.Setup(data);

        skills.Clear();
        if (data.skills != null && data.skills.Count > 0)
            skills.AddRange(data.skills);
        else
            Debug.LogWarning($"{data.unitName} chưa có skill nào trong UnitData!");

        currentAP = data.maxAP;
        InitAPBar();
        UpdateAP(currentAP);
    }

    //  ========================================== SELECT =================================================
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

    //  =========================================== DESELECT ==============================================
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

    // ================================================ MOVE =============================================
    public override void MoveTo(Vector3 worldPos, Vector2Int gridPos)
    {
        if (!HasEnoughAP(1))
        {
            Debug.Log("Không đủ AP để di chuyển!");
            return;
        }

        base.MoveTo(worldPos, gridPos);
        SpendAP(1);
    }

    // ================================================ AP SYSTEM ========================================
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
            GameObject apIcon = i < value ? apPrefab : emptyApPrefab;
            Instantiate(apIcon, apContainer);
        }

        if (SelectedHero != null)
            UIManager.Instance.ShowSkillBar(this);
    }


    public bool HasEnoughAP(int amount) => currentAP >= amount;

    public void SpendAP(int amount)
    {
        currentAP = Mathf.Max(0, currentAP - amount);
        UpdateAP(currentAP);
        UIManager.Instance.ShowSkillBar(this);
    }

    public void RefillAP()
    {
        currentAP = data.maxAP;
        UpdateAP(currentAP);
    }

    //=============================================== ARROW =============================================
    private void ShowArrow()
    {
        if (arrowInstance == null)
        {
            arrowInstance = Instantiate(Resources.Load<GameObject>("Prefabs/ArrowUI"));
            ArrowFollowUnit follow = arrowInstance.GetComponentInChildren<ArrowFollowUnit>(); 
            if (follow != null)
            {
                follow.target = transform;
            }
        }

        arrowInstance.SetActive(true);
    }

    private void HideArrow()
    {
        if (arrowInstance != null)
            arrowInstance.SetActive(false);
    }

    // =========================================== TILE HIGHLIGHT ========================================
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

    // ============================================= ACCESS ==============================================
    public List<SkillData> GetSkills() => skills;
}
