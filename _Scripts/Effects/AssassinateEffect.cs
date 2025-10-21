using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/Effects/Assassinate")]
public class AssassinateEffect : EffectData
{
    public override GameAction CreateAction(Unit caster, Unit target)
    {
        return new AssassinateGA(caster, target); 
    }
}