using System.Collections;
using UnityEngine;

public class AssassinateGA : GameAction
{
    public AssassinateGA(Unit caster, Unit target, bool isPassive = false)
    {
        Caster = caster;
        Target = target;
        IsPassiveAction = isPassive;
    }
}
