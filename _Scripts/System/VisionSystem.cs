using System.Collections.Generic;
using UnityEngine;

public class VisionSystem : Singleton<VisionSystem>
{
     public void ApplyDiamondVision(Vector2Int center, int range)
    {
        foreach (var kv in GridManager.Instance.tiles)
        {
            Vector2Int p = kv.Key;
            Tile tile = kv.Value;

            int manhattan = GridManager.Instance.GetDistance(center, p);
            if (manhattan > range) continue;

            tile.AddVision();
            Debug.Log($"[Vision] Sáng ở {p} từ tâm {center} (Manhattan = {manhattan})");
        }
    }

    private void ApplyVision(Vector2Int center, int range, bool add)
    {
        foreach (var kv in GridManager.Instance.tiles)
        {
            Vector2Int p = kv.Key;
            Tile tile = kv.Value;

            int dx = Mathf.Abs(p.x - center.x);
            int dy = Mathf.Abs(p.y - center.y);
            if (Mathf.Max(dx, dy) > range) continue;


            if (HasLineOfSight(center, p))
            {
                if (add) tile.AddVision();
                else tile.RemoveVision();
            }
        }
    }

    private bool HasLineOfSight(Vector2Int a, Vector2Int b)
    {
        foreach (var pos in BresenhamLine(a, b))
        {
            if (pos == a || pos == b) continue;
            Tile t = GridManager.Instance.GetTileAtPosition(pos);
            if (t != null && t.IsObstacle) return false;
        }
        return true;
    }

    private IEnumerable<Vector2Int> BresenhamLine(Vector2Int a, Vector2Int b)
    {
        int x0 = a.x, y0 = a.y;
        int x1 = b.x, y1 = b.y;

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            yield return new Vector2Int(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
}
