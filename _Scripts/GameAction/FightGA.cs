using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightGA : GameAction
{
    public FightGA(Unit caster, Unit target)
    {
        Caster = caster;
        Target = target;
    }
}
