using UnityEngine;
using System.Collections.Generic; 

public enum TargetType { None, SingleUnit, Line, DiamondAOE, Self } // Thêm Self

public abstract class TargetingData : ScriptableObject
{
    public TargetType type;
    public int range = 1;
    public bool requiresEnemy;
    public bool requiresHero;

    public abstract bool IsTargetValid(Unit caster, Vector2Int targetPos);
    
    public abstract void HighlightTargets(Unit caster);
    public abstract void ClearHighlights();
    public abstract List<Unit> GetValidTargets(Unit caster); 
}