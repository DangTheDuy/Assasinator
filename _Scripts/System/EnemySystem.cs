
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
        ActionSystem.AttachPerformer<AssassinateGA>(AssassinatePerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<EnemyMoveGA>();
        ActionSystem.DetachPerformer<AssassinateGA>();
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

        List<EnemyUnit> enemies = GetAllEnemies();
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

    private IEnumerator AssassinatePerformer(AssassinateGA assassinateGA)
    {
        EnemyUnit targetEnemy = assassinateGA.Target as EnemyUnit; 
        if (targetEnemy != null)
        {
            Tile occupiedTile = GridManager.Instance.GetTileAtPosition(targetEnemy.currentPosition);
            if (occupiedTile != null)
            {
                occupiedTile.SetUnoccupied(targetEnemy); 
            }

            Destroy(targetEnemy.gameObject);
            UIManager.Instance.HideSkillBar();
            if (Unit.SelectedHero != null)
            {
                Unit.SelectedHero.OnDeselect();   // gọi hàm deselect để clear logic + highlight
                Unit.SelectedHero = null;
            }

            Debug.Log($"Enemy {targetEnemy.name} đã bị ám sát và xoá khỏi ô {targetEnemy.currentPosition}");
        }
        else
        {
            Debug.LogWarning("AssassinateGA không có targetEnemy!");
        }

        yield break;
    }

}
