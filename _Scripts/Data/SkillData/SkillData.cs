// File: SkillData.cs (Sửa đổi)
using UnityEngine;

public enum SkillType { Damage, Utility, Healing, Movement }

[CreateAssetMenu(menuName = "Skill System/Skill Data (Base)")]
public class SkillData : ScriptableObject
{
    public string skillName = "New Skill";
    public Sprite icon;
    [TextArea] public string description;
    public SkillType type = SkillType.Damage;
    public int apCost = 1;
    
    [Header("Skill Components")]
    // 💡 Liên kết với Target Type và Effect Type (SRP & OCP)
    public TargetingData targeting; 
    public EffectData[] effects; 
}