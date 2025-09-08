using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/ShurikenSkill")]
public class ShurikenSkill : TargetSkill
{
    public override void Execute(Unit caster, Unit target)
    {
        ActionSystem.Instance.Perform(new ShurikenGA(caster, target));
    }


}

