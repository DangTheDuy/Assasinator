using UnityEngine;
using System.Collections.Generic;

public class SkillDatabase : Singleton<SkillDatabase>
{
    private Dictionary<string, SkillData> skills = new Dictionary<string, SkillData>();

    protected override void Awake()
    {
        base.Awake();
        LoadAllSkills();
    }   
    private void LoadAllSkills()
    {
        SkillData[] allSkills = Resources.LoadAll<SkillData>("Skills");
        foreach (var s in allSkills)
        {
            skills[s.name.ToLower()] = s; // key = tên file/ID
        }
    }

    public SkillData GetSkill(string id)
    {
        id = id.ToLower();
        if (skills.ContainsKey(id)) return skills[id];
        Debug.LogWarning($"Skill {id} not found in database");
        return null;
    }
}
