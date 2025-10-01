using UnityEngine;

public class MountainTile : Tile
{
    public override int MovementCost => 2;  
    public override bool IsObstacle => false; 


    public override void OnUnitEnter(Unit unit)
    {
        base.OnUnitEnter(unit);

        if (unit is HeroUnit hero)
        {
            hero.visionRange += 1;
            VisionSystem.Instance.UpdateDiamondVision(hero.currentPosition, hero.visionRange, null);
        }
    }

    public override void OnUnitExit(Unit unit)
    {
        base.OnUnitExit(unit);

        if (unit is HeroUnit hero)
        {
            hero.visionRange = Mathf.Max(1, hero.visionRange - 1);
        }
        RecalculateAllHeroesVision();
    }

    private void RecalculateAllHeroesVision()
    {
        foreach (var kv in GridManager.Instance.tiles)
        {
            kv.Value.RemoveVision();
        }
        foreach (var hero in Object.FindObjectsOfType<HeroUnit>())
        {
            if (!hero.IsDead)
            {
                VisionSystem.Instance.UpdateDiamondVision(hero.currentPosition, hero.visionRange, null);
            }
        }
    }

    public bool BlocksVision(Vector2Int from, Vector2Int to)
    {
        if (from == gridPosition)
            return false;
        if (Mathf.Min(from.y, to.y) <= gridPosition.y && Mathf.Max(from.y, to.y) >= gridPosition.y)
        {
            if (from.x < gridPosition.x && to.x > gridPosition.x)
                return true;
            if (from.x > gridPosition.x && to.x < gridPosition.x)
                return true;
        }

        return false;
    }
}
