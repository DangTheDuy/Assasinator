using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDiedGA : GameAction
{
    public Unit deadUnit;

    public UnitDiedGA(Unit unit)
    {
        deadUnit = unit;
    }
}
