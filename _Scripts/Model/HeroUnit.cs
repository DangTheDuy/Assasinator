using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroUnit : Unit
{
    public List<SkillData> skills = new List<SkillData>();
    private UnitData unitData;

    private SkillBarUI skillBar;

    public override void Setup(UnitData data)
    {
        base.Setup(data);
        skills.Clear();

        if (data.skills != null && data.skills.Count > 0)
        {
            skills.AddRange(data.skills);
        }
        else
        {
            Debug.LogWarning($"{data.unitName} chưa có skill nào trong UnitData!");
        }
    }

    public override void OnSelect()
    {
        base.OnSelect();
        foreach (var skill in skills)
        {
            Debug.Log($"Hero có skill: {skill.skillName}");
        }
        UIManager.Instance.ShowSkillBar(this);
        List<Vector2Int> tilesInRange = new List<Vector2Int>();
        foreach (var kv in GridManager.Instance.tiles)
        {
            int distance = GridManager.Instance.GetDistance(currentPosition, kv.Key);
            if (distance <= data.moveRange && GridManager.Instance.IsCellAvailableForMovement(kv.Key))
            {
                kv.Value.Highlight(true);
            }
        }
    }

    public override void OnDeselect()
    {
        base.OnDeselect();
        UIManager.Instance.HideSkillBar();
        foreach (var kv in GridManager.Instance.tiles)
        {
            kv.Value.Highlight(false);
        }
    }
    
    public List<SkillData> GetSkills()
    {
        return skills;
    }
}

