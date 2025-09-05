using UnityEngine;

public class ShurikenGA : GameAction
{
    public Unit Caster;
    public Unit Target;

    public ShurikenGA(Unit caster, Unit target)
    {
        Caster = caster;
        Target = target;
    }
}
