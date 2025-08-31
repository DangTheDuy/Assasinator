using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Assassinate")]
public class AssassinateSkill : SkillData
{
    public override void Execute(Unit caster, Unit target)
    {
        Debug.Log("Execute Assasinate");
            ActionSystem.Instance.Perform(new AssassinateGA(caster, target));
    }
}
