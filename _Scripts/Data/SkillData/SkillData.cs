using UnityEngine;

//===================== SKILL TYPE ===========================
public enum SkillType { Damage, Utility, Healing, Movement }

//===================== SKILL WRAPPER ===========================

[System.Serializable]
public class SkillEffectWrapper // 🚨 LỚP MỚI: NHÓM EFFECT VÀ GIÁ TRỊ
{
    public EffectData effect;

    [Header("Custom Values (Optional)")]
    public int baseDamage = 0;        // Tùy chỉnh Dame gốc
    public float damageMultiplier = 1f; // Tùy chỉnh Tỷ lệ
}

//===================== SKILL DATA ===========================

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
    public SkillEffectWrapper[] wrappedEffects;
}