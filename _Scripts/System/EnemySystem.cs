
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
    }

    public List<Unit> GetAllEnemies()
    {
        // TODO: return danh sách unit kẻ địch
        return new List<Unit>(FindObjectsOfType<Unit>()); 
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        yield break;
    }

    private IEnumerator EnemyMovePerformer(EnemyMoveGA enemyMoveGA)
    {
        Vector2Int dir = enemyMoveGA.direction;

        List<Unit> enemies = GetAllEnemies();
        foreach (var enemy in enemies)
        {
            Vector2Int target = enemy.currentPosition + dir;
            Tile targetTile = GridManager.Instance.GetTileAtPosition(target);

            if (targetTile != null && !targetTile.IsObstacle)
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(target);
                enemy.MoveTo(worldPos, target);
            }
        }

        yield break;
    }

}
