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
    private Tween arrowTween;
    private int currentAP;

    // SETUP ============================================================================================
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

    // SELECT ===========================================================================================
    public override void OnSelect()
    {
        base.OnSelect();

        if (SelectedHero != null && SelectedHero != this)
            SelectedHero.OnDeselect();

        SelectedHero = this;
        UIManager.Instance.ShowSkillBar(this);

        ShowArrow();
        HighlightMovementTiles();
    }

    // DESELECT =========================================================================================
    public override void OnDeselect()
    {
        base.OnDeselect();

        if (SelectedHero == this)
            SelectedHero = null;

        UIManager.Instance.HideSkillBar();
        HideArrow();
        ClearTileHighlights();
    }

    // MOVE =============================================================================================
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

    // AP SYSTEM ========================================================================================
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
    }


    public bool HasEnoughAP(int amount) => currentAP >= amount;

    public void SpendAP(int amount)
    {
        currentAP = Mathf.Max(0, currentAP - amount);
        UpdateAP(currentAP);
    }

    public void RefillAP()
    {
        currentAP = data.maxAP;
        UpdateAP(currentAP);
    }

    // ARROW ============================================================================================
    private void ShowArrow()
    {
        if (arrow == null) return;

        arrow.SetActive(true);
        arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
        arrowTween?.Kill();

        arrowTween = arrow.transform.DOLocalMoveY(1.0f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void HideArrow()
    {
        if (arrow == null) return;

        arrowTween?.Kill();
        arrow.SetActive(false);
    }

    // TILE HIGHLIGHT ===================================================================================
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

    // ACCESS ===========================================================================================
    public List<SkillData> GetSkills() => skills;
}
