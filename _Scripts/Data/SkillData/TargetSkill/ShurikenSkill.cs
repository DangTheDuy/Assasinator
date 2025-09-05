using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/ShurikenSkill")]
public class ShurikenSkill : TargetSkill
{
    public override void Execute(Unit caster, Unit target)
{
    Debug.Log($"[ShurikenSkill] Execute: caster={caster?.name} target={target?.name}");
    ActionSystem.Instance.Perform(new ShurikenGA(caster, target));
}


}

