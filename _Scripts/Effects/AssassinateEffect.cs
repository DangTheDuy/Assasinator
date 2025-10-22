using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/Effects/Assassinate")]
public class AssassinateEffect : EffectData
{
    public override GameAction CreateAction(Unit caster, Unit target, int customValue, float customMultiplier)
    {
        if (target == null) return null;
        return new AssassinateGA(caster, target); 
    }
}