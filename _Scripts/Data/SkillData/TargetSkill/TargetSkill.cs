using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetSkill : SkillData
{
    public override void Execute(Unit caster, Unit target)
    {
        if (target == null)
        {
            Debug.LogWarning("TargetSkill cần có target!");
            return;
        }
    }
}

