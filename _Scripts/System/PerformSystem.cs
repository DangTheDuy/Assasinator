using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PerformSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AssassinateGA>(AssassinatePerformer);
        ActionSystem.AttachPerformer<DamageGA>(DamagePerformer);
        ActionSystem.AttachPerformer<FightGA>(FightPerformer);        
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AssassinateGA>();
        ActionSystem.DetachPerformer<DamageGA>();
        ActionSystem.DetachPerformer<FightGA>();        
        ActionSystem.DetachPerformer<AttackHeroGA>();
    }
    
    // ================== PERFOMERS CŨ (ĐÃ SỬA) ==================

    // 🛠️ FIGHT PERFORMER MỚI: Chỉ dùng cho hiệu ứng cận chiến có va chạm/phản đòn
    private IEnumerator FightPerformer(FightGA fightGA)
    {
        int damageToTarget = fightGA.Caster.AttackPower;
        int damageToCaster = fightGA.Target.AttackPower;

        // ... (Logic animation) ...

        fightGA.Target.TakeDamage(damageToTarget);
        fightGA.Caster.TakeDamage(damageToCaster);
        
        // ... (Logic cập nhật UI và cleanup) ...
        yield return new WaitForSeconds(0.3f);
    }
    
    // ================== PERFOMER MỚI ==================
    private IEnumerator DamagePerformer(DamageGA damageGA)
    {
        if (damageGA.Caster == null || damageGA.Target == null || damageGA.Target.IsDead) yield break;

        // 1. Animation đơn giản (ví dụ: chỉ lắc mục tiêu)
        yield return damageGA.Target.transform.DOShakePosition(
            duration: 0.2f, 
            strength: new Vector3(0.15f, 0.15f, 0), 
            vibrato: 20, 
            randomness: 90, 
            snapping: false, 
            fadeOut: true
        ).WaitForCompletion();

        // 2. Gây sát thương cấu hình
        damageGA.Target.TakeDamage(damageGA.DamageAmount);

        // 3. Cập nhật UI
        if (damageGA.Target is HeroUnit heroTarget)
            heroTarget.UpdateHUD();

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator AssassinatePerformer(AssassinateGA assassinateGA)
    {
        yield return ExecuteKill(assassinateGA.Target);
        if (Unit.SelectedHero != null)
        {
            UIManager.Instance.ShowSkillBar(Unit.SelectedHero);
        }
    }

    private IEnumerator ExecuteKill(Unit target)
    {
        if (target == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.2f);
        target.Die();
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA action)
    {
        if (action.attacker == null || action.target == null || action.target.IsDead) yield break;

        Vector3 startPos = action.attacker.transform.position;
        Vector3 targetPos = action.target.transform.position;

        yield return action.attacker.transform.DOMove(targetPos, 0.25f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return action.target.transform.DOShakePosition(
            duration: 0.2f, 
            strength: new Vector3(0.15f, 0.15f, 0), 
            vibrato: 20, 
            randomness: 90, 
            snapping: false, 
            fadeOut: true
        ).WaitForCompletion();

        yield return action.attacker.transform.DOMove(startPos, 0.25f).SetEase(Ease.InQuad).WaitForCompletion();

        int damage = action.attacker.AttackPower;
        action.target.TakeDamage(damage);
        if (action.target is HeroUnit heroTarget)
        heroTarget.UpdateHUD();
        yield return new WaitForSeconds(0.2f);
    }

}
