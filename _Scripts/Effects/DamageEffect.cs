// File: DamageEffect.cs (MỚI)
using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/Effects/Damage")]
public class DamageEffect : EffectData
{
    public int baseDamage = 0;
    public int damageMultiplier = 1;

    public override GameAction CreateAction(Unit caster, Unit target)
    {
        int calculatedDamage = baseDamage + Mathf.RoundToInt(caster.AttackPower * damageMultiplier);
        return new DamageGA(caster, target, calculatedDamage); 
    }
}