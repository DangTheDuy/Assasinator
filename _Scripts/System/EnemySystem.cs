
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySystem : Singleton<EnemySystem>
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<EnemyMoveGA>(EnemyMovePerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<EnemyMoveGA>();
    }

    public List<EnemyUnit> GetAllEnemies()
    {
        return new List<EnemyUnit>(FindObjectsOfType<EnemyUnit>());
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        yield break;
    }

    private IEnumerator EnemyMovePerformer(EnemyMoveGA enemyMoveGA)
    {
        Vector2Int dir = enemyMoveGA.direction;
        List<EnemyUnit> allEnemies = GetAllEnemies();

        List<EnemyUnit> firstWave = new List<EnemyUnit>();
        List<EnemyUnit> secondWave = new List<EnemyUnit>();

        // ======= LƯỢT 1: Di chuyển nếu tile đích còn chỗ =======
        foreach (var enemy in allEnemies)
        {
            Vector2Int target = enemy.currentPosition + dir;
            Tile targetTile = GridManager.Instance.GetTileAtPosition(target);
            if (targetTile == null || targetTile.IsObstacle)
                continue;

            int enemyCount = targetTile.occupyingUnits.FindAll(u => u is EnemyUnit).Count;
            if (enemyCount < 4 && targetTile.CanAccept(enemy))
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(target);
                enemy.MoveTo(worldPos, target);
                firstWave.Add(enemy);
            }
            else
            {
                secondWave.Add(enemy); // tile đầy → thử lại sau
            }
        }

        // ======= LƯỢT 2: Thử lại với các enemy bị từ chối =======
        foreach (var enemy in secondWave)
        {
            Vector2Int target = enemy.currentPosition + dir;
            Tile targetTile = GridManager.Instance.GetTileAtPosition(target);
            if (targetTile == null || targetTile.IsObstacle)
                continue;

            int enemyCount = targetTile.occupyingUnits.FindAll(u => u is EnemyUnit).Count;
            if (enemyCount < 4 && targetTile.CanAccept(enemy))
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(target);
                enemy.MoveTo(worldPos, target);
            }
            else
            {
                Debug.LogWarning($"Enemy {enemy.name} không thể di chuyển đến {target} cả sau lượt 2.");
            }
        }

        yield break;
    }


}
