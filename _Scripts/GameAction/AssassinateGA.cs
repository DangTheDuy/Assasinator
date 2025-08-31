using System.Collections;
using UnityEngine;

public class AssassinateGA : GameAction
{
    public Unit Caster { get; private set; }
    public Unit Target { get; private set; }

    public AssassinateGA(Unit caster, Unit target)
    {
        Caster = caster;
        Target = target;
    }
}
