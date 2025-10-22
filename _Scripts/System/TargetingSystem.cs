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
        if (currentSkill.wrappedEffects != null)
        {
            foreach (var wrapper in currentSkill.wrappedEffects)
            {
                if (wrapper?.effect == null) continue;
                GameAction action = wrapper.effect.CreateAction(
                    caster, 
                    target, 
                    wrapper.baseDamage, 
                    wrapper.damageMultiplier
                );

                if (action != null)
                    ActionSystem.Instance.Perform(action);
            }
        }
        if (currentItemStack != null)
        {
            caster.UseItem(currentItemStack); 
            currentItemStack = null;
        }
    }
}