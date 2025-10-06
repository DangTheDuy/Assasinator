using System.Collections.Generic;
using UnityEngine;

public class VisionSystem : Singleton<VisionSystem>
{
    private Dictionary<HeroUnit, HashSet<Tile>> heroVisibleTiles = new Dictionary<HeroUnit, HashSet<Tile>>();

    public void UpdateDiamondVision(Vector2Int newPos, int range, Vector2Int? oldPos = null, int oldRange = -1, HeroUnit hero = null)
    {
        if (hero == null) return;
        if (GridManager.Instance == null || GridManager.Instance.tiles.Count == 0) return;

        // Lấy vùng nhìn cũ
        HashSet<Tile> oldTiles;
        if (!heroVisibleTiles.TryGetValue(hero, out oldTiles))
            oldTiles = new HashSet<Tile>();

        // Tạo vùng nhìn mới
        HashSet<Tile> newTiles = new HashSet<Tile>();

        foreach (var kv in GridManager.Instance.tiles)
        {
            Vector2Int p = kv.Key;
            Tile tile = kv.Value;
            int manhattan = GridManager.Instance.GetDistance(newPos, p);
            if (manhattan > range) continue;

            if (HasLineOfSight(newPos, p))
                newTiles.Add(tile);
        }

        // Cập nhật vùng riêng của hero (so sánh cũ/mới)
        heroVisibleTiles[hero] = newTiles;
        RecalculateAllVision();
    }

    public void RemoveHeroVision(HeroUnit hero)
    {
        if (heroVisibleTiles.Remove(hero))
            RecalculateAllVision();
    }

    public void RecalculateAllVision()
    {
        // Reset toàn bộ tile bằng cách gọi RemoveVision cho tới khi count = 0
        foreach (var tile in GridManager.Instance.tiles.Values)
        {
            while (tile.visibleCount > 0)
                tile.RemoveVision();
        }

        // Cộng dồn tầm nhìn của mọi hero
        foreach (var kv in heroVisibleTiles)
        {
            var hero = kv.Key;
            if (hero == null) continue;

            foreach (var tile in kv.Value)
                tile.AddVision();
        }
    }

    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        if (from == to) return true;

        foreach (var pos in BresenhamLine(from, to))
        {
            if (pos == from || pos == to) continue;
            Tile t = GridManager.Instance.GetTileAtPosition(pos);
            if (t == null) continue;
            if (t.IsObstacle && !(t is WaterTile))
                return false;
        }

        // nếu có tile đặc biệt (núi chẳng hạn) chặn tầm nhìn
        foreach (var kv in GridManager.Instance.tiles)
        {
            if (kv.Value is MountainTile mountain)
            {
                if (mountain.BlocksVision(from, to))
                    return false;
            }
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

    public bool IsTileInVision(EnemyUnit enemy, Vector2Int tilePos)
    {
        if (enemy == null) return false;
        int dist = GridManager.Instance.GetDistance(enemy.currentPosition, tilePos);
        return dist <= enemy.visionRange;
    }

    public void CheckHeroInEnemyVision(HeroUnit hero)
    {
        if (hero == null || hero.IsDead) return;
        var allEnemies = EnemySystem.Instance.GetAllEnemies();
        if (allEnemies == null || allEnemies.Count == 0) return;

        foreach (var e in allEnemies)
        {
            if (e == null || e.IsDead) continue;
            if (e.currentState == EnemyState.Chase)
            {
                return;
            }
        }

        Tile heroTile = GridManager.Instance.GetTileAtPosition(hero.currentPosition);
        if (heroTile == null) return;

        bool inEnemyVision = false;
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (IsTileInVision(enemy, hero.currentPosition))
            {
                inEnemyVision = true;
                break;
            }
        }
        if (inEnemyVision)
            heroTile.CheckDetection();
    }

}
