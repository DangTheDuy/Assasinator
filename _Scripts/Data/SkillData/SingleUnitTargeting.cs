using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Skill System/Targeting/Single Unit")]
public class SingleUnitTargeting : TargetingData
{
    public override bool IsTargetValid(Unit caster, Vector2Int targetPos)
    {
        int distance = GridManager.Instance.GetDistance(caster.currentPosition, targetPos);
        if (distance > caster.AttackRange) return false;

        Tile targetTile = GridManager.Instance.GetTileAtPosition(targetPos);
        if (targetTile == null || targetTile.occupyingUnits.Count == 0) return false;

        if (requiresEnemy)
            return targetTile.occupyingUnits.Exists(u => u is EnemyUnit);
        if (requiresHero)
            return targetTile.occupyingUnits.Exists(u => u is HeroUnit);
        return true; 
    }
    
    public override void HighlightTargets(Unit caster)
    {
        foreach (Unit unit in Unit.AllUnits)
        {
            if (IsTargetValid(caster, unit.currentPosition))
            {
                if (unit is EnemyUnit enemy)
                    enemy.SetHighlight(true); 
            }
        }
    }
    
    public override void ClearHighlights()
    {
        foreach (Unit unit in Unit.AllUnits)
        {
            if (unit is EnemyUnit enemy)
            {
                enemy.SetHighlight(false);
            }
        }
    }
    
    public override List<Unit> GetValidTargets(Unit caster)
    {
        List<Unit> validTargets = new List<Unit>();
        foreach (Unit unit in Unit.AllUnits)
        {
            if (IsTargetValid(caster, unit.currentPosition))
            {
                validTargets.Add(unit);
            }
        }
        return validTargets;
    }
}