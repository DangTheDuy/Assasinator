using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PerformSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AssassinateGA>(AssassinatePerformer);
        ActionSystem.AttachPerformer<ShurikenGA>(ShurikenPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<FightGA>(FightPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AssassinateGA>();
        ActionSystem.DetachPerformer<ShurikenGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<FightGA>();
    }

    private IEnumerator AssassinatePerformer(AssassinateGA assassinateGA)
    {
        yield return ExecuteKill(assassinateGA.Target);
        if (Unit.SelectedHero != null)
        {
            UIManager.Instance.ShowSkillBar(Unit.SelectedHero);
        }
    }

    private IEnumerator FightPerformer(FightGA fightGA)
    {
        if (fightGA.Caster == null || fightGA.Target == null) yield break;
        if (fightGA.Caster.IsDead || fightGA.Target.IsDead) yield break;

        //  Tính sát thương trước
        int damageToTarget = fightGA.Caster.AttackPower;
        int damageToCaster = fightGA.Target.AttackPower;

        Debug.Log($"{fightGA.Caster.name} và {fightGA.Target.name} tấn công lẫn nhau!");

        yield return new WaitForSeconds(0.2f); 

        //  Áp dụng sát thương cùng lúc
        fightGA.Target.TakeDamage(damageToTarget);
        fightGA.Caster.TakeDamage(damageToCaster);

        Debug.Log($"{fightGA.Caster.name} gây {damageToTarget} sát thương cho {fightGA.Target.name}");
        Debug.Log($"{fightGA.Target.name} gây {damageToCaster} sát thương cho {fightGA.Caster.name}");

        yield return new WaitForSeconds(0.3f); 
    }


    private IEnumerator ShurikenPerformer(ShurikenGA shurikenGA)
    {
        yield return ExecuteKill(shurikenGA.Target);
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

        int damage = action.attacker.AttackPower;
        action.target.TakeDamage(damage);

        yield return new WaitForSeconds(0.2f);
    }
}
