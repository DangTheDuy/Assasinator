// File: DamageGA.cs
using UnityEngine;

public class DamageGA : GameAction
{
    public Unit Caster { get; private set; }
    public Unit Target { get; private set; }
    public int DamageAmount { get; private set; }

    public DamageGA(Unit caster, Unit target, int damageAmount)
    {
        Caster = caster;
        Target = target;
        DamageAmount = damageAmount;
    }
}