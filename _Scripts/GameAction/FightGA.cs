using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightGA : GameAction
{
    public Unit Caster { get; private set; }
    public Unit Target { get; private set; }

    public FightGA(Unit caster, Unit target)
    {
        Caster = caster;
        Target = target;
    }
}
