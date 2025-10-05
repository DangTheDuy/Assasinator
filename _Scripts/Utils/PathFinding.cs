using System.Collections.Generic;
using UnityEngine;

public static class PathfindingHelper
{
    public static List<Vector2Int> GetStepsToward(Vector2Int start, Vector2Int target, int steps)
    {
        List<Vector2Int> path = FindPath(start, target);
        if (path == null || path.Count <= 1)
            return new List<Vector2Int> { start };

        // lấy tối đa số bước cho phép (vd: 2)
        int takeCount = Mathf.Min(steps, path.Count - 1);
        return path.GetRange(1, takeCount);
    }

    private static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var grid = GridManager.Instance;
        if (grid == null)
            return null;

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (var dir in GridManager.Directions)
            {
                var next = current + dir;

                if (!grid.tiles.ContainsKey(next))
                    continue;

                var tile = grid.tiles[next];
                if (tile.IsObstacle)
                    continue;

                if (cameFrom.ContainsKey(next))
                    continue;

                cameFrom[next] = current;
                frontier.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return null;

        // reconstruct path
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int currentPos = goal;

        while (currentPos != start)
        {
            path.Add(currentPos);
            currentPos = cameFrom[currentPos];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
}
