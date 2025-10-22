using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Skill System/Targeting/Line")]
public class LineTargeting : TargetingData
{
    // Cấu hình: True nếu skill tác động lên tất cả mục tiêu trên đường thẳng
    public bool targetsAllInLine = false;

    public override bool IsTargetValid(Unit caster, Vector2Int targetPos)
    {
        int distance = GridManager.Instance.GetDistance(caster.currentPosition, targetPos);
        if (distance == 0 || distance > range) return false;

        // 2. Kiểm tra mục tiêu có nằm trên cùng hàng/cột không
        bool isHorizontal = targetPos.y == caster.currentPosition.y;
        bool isVertical = targetPos.x == caster.currentPosition.x;

        if (!isHorizontal && !isVertical) return false;

        Tile targetTile = GridManager.Instance.GetTileAtPosition(targetPos);
        if (targetTile == null) return false;

        // 3. Kiểm tra loại mục tiêu (Enemy/Hero)
        if (requiresEnemy && !targetTile.occupyingUnits.Exists(u => u is EnemyUnit))
        {
            if (!targetsAllInLine) return false;
        }
        if (requiresHero && !targetTile.occupyingUnits.Exists(u => u is HeroUnit))
        {
            if (!targetsAllInLine) return false;
        }

        return true;
    }
    
    public override void HighlightTargets(Unit caster)
    {
        // 1. Highlight toàn bộ đường thẳng (hàng hoặc cột) trong phạm vi
        for (int i = 1; i <= range; i++)
        {
            // Highlight theo hàng ngang
            HighlightTile(new Vector2Int(caster.currentPosition.x + i, caster.currentPosition.y), caster);
            HighlightTile(new Vector2Int(caster.currentPosition.x - i, caster.currentPosition.y), caster);
            
            // Highlight theo hàng dọc
            HighlightTile(new Vector2Int(caster.currentPosition.x, caster.currentPosition.y + i), caster);
            HighlightTile(new Vector2Int(caster.currentPosition.x, caster.currentPosition.y - i), caster);
        }
    }

    private void HighlightTile(Vector2Int pos, Unit caster)
    {
        if (IsTargetValid(caster, pos))
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(pos);
            if (tile != null)
            {
                if (!targetsAllInLine && tile.occupyingUnits.Count > 0)
                {
                    tile.occupyingUnits.ForEach(u => (u as EnemyUnit)?.SetHighlight(true));
                }
                else
                {
                    tile.Highlight(true); 
                }
            }
        }
    }

    public override void ClearHighlights()
    {
        foreach (var kv in GridManager.Instance.tiles)
        {
            kv.Value.Highlight(false);
            kv.Value.occupyingUnits.ForEach(u => (u as EnemyUnit)?.SetHighlight(false));
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