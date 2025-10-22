using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/Effects/Damage")]
public class DamageEffect : EffectData
{
    public override GameAction CreateAction(Unit caster, Unit target, int customValue, float customMultiplier)
    {
        if (target == null) return null;
        
        int baseDamage = Mathf.RoundToInt(caster.AttackPower * customMultiplier) ;
        int finalDamage = baseDamage + customValue;
        return new DamageGA(caster, target, finalDamage);
    }
}