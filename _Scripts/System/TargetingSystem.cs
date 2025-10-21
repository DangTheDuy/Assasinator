// File: TargetingSystem.cs (Sửa đổi)
using UnityEngine;

public class TargetingSystem : Singleton<TargetingSystem>
{
    private SkillData currentSkill;
    private ItemStack currentItemStack;
    private HeroUnit caster;
    public bool IsTargeting => currentSkill != null;

    public void EnterTargetMode(HeroUnit casterUnit, SkillData skill, ItemStack stack = null)
    {
        currentSkill = skill;
        currentItemStack = stack;
        caster = casterUnit;

        // 💡 SỬA: Dùng TargetingData để highlight
        currentSkill.targeting?.HighlightTargets(caster);
    }


    public void ExitTargetMode()
    {
        // 💡 SỬA: Dùng TargetingData để xóa highlight
        currentSkill?.targeting?.ClearHighlights(); 
        
        currentSkill = null;
        caster = null;
        currentItemStack = null;
        
        SkillBarUI.Instance?.ResetSelectedSkill();
    }

    public void TrySelectEnemy(EnemyUnit enemy)
    {
        if (currentSkill == null || caster == null || enemy == null) return;

        // 💡 SỬA: Dùng TargetingData để kiểm tra tính hợp lệ
        if (currentSkill.targeting.IsTargetValid(caster, enemy.currentPosition))
        {
            // 💡 SỬA: Sử dụng ActionFactory để thực thi (hoặc Execute mới)
            ExecuteSkill(caster, enemy);
            
            ExitTargetMode();
        }
    }
    
    private void ExecuteSkill(HeroUnit caster, Unit target)
    {
        if (!caster.HasEnoughAP(currentSkill.apCost)) return;

        // Xử lý Cost (AP và Item)
        // ... (Logic SpendAP, Consume Item như cũ) ...
        
        // 💡 SỬA: Thực thi Action cho từng Effect
        foreach (var effect in currentSkill.effects)
        {
            GameAction action = effect.CreateAction(caster, target);
            if (action != null)
                ActionSystem.Instance.Perform(action);
        }
    }
}