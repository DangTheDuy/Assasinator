using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectData : ScriptableObject
{
    public abstract GameAction CreateAction(Unit caster, Unit target);
}
