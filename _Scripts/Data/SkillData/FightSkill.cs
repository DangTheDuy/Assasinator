using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Fight")]
public class FightSkill : SkillData
{
    public override void Execute(Unit caster, Unit target)
    {
        Debug.Log("Execute Fight");
            ActionSystem.Instance.Perform(new FightGA(caster, target));
    }
}
