using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridUtils
{
    public static List<Vector2Int> GetCellsInRange(Vector2Int origin, int range)
    {
        List<Vector2Int> cellsInRange = new List<Vector2Int>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (Mathf.Abs(dx) + Mathf.Abs(dy) <= range)
                {
                    Vector2Int cell = new Vector2Int(origin.x + dx, origin.y + dy);
                    
                    if (cell.x >= 0 && cell.x < 10 && cell.y >= 0 && cell.y < 5)
                    {
                        cellsInRange.Add(cell);
                        // Debug.Log("Ô trong phạm vi: " + cell);
                    }
                }
            }
        }
        return cellsInRange;
    }

   // public static List<Vector2Int> GetTilesInLine(Vector2Int start, Vector2Int direction, int length)
   public static bool IsValidPosition(Vector2Int position)
    {
        return GridManager.Instance.tiles.ContainsKey(position); 
    }
    public static Tile GetTileAtPosition(Vector2Int position)
    {
        return GridManager.Instance.GetTileAtPosition(position); 
    }

    public static Unit GetUnitAtPosition(Vector2Int position)
    {
        Tile tile = GridManager.Instance.GetTileAtPosition(position);
        return null;
    }

    
}
