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
    }

    public override void OnDeSelect()
    {
        base.OnDeSelect();
        UIManager.Instance.HideSkillBar();
    }
    
    public List<SkillData> GetSkills()
    {
        return skills;
    }
}

