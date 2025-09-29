
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

        // ======= LƯỢT 1: Nếu đang cùng hero thì attack, nếu không thì move =======
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            Tile currentTile = GridManager.Instance.GetTileAtPosition(enemy.currentPosition);
            if (currentTile == null) continue;

            // 🔹 Nếu trên tile hiện tại có hero -> attack, bỏ qua move
            HeroUnit heroOnTile = currentTile.occupyingUnits.Find(u => u is HeroUnit && !u.IsDead) as HeroUnit;
            if (heroOnTile != null)
            {
                heroOnTile.IsDetected = true;
                HeroAlertUI.Instance?.SetDetected(true);

                ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy, heroOnTile));
                Debug.Log($"🚨 {enemy.name} attack {heroOnTile.name} (không move vì cùng tile)");

                continue; // bỏ qua move
            }

            // 🔹 Nếu không có hero thì xử lý move như cũ
            Vector2Int target = enemy.currentPosition + dir;
            Tile targetTile = GridManager.Instance.GetTileAtPosition(target);
            if (targetTile == null || targetTile.IsObstacle)
                continue;

            int enemyCount = targetTile.occupyingUnits.FindAll(u => u is EnemyUnit).Count;
            if (enemyCount < 4 && targetTile.CanAccept(enemy))
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(target);
                enemy.MoveTo(worldPos, target);
                enemy.OnEnterTile(targetTile);
                firstWave.Add(enemy);
            }
            else
            {
                secondWave.Add(enemy); // tile đầy → thử lại sau
            }
        }

        // ======= LƯỢT 2: Enemy bị từ chối thử move lại =======
        foreach (var enemy in secondWave)
        {
            if (enemy == null || enemy.IsDead) continue;

            Tile currentTile = GridManager.Instance.GetTileAtPosition(enemy.currentPosition);
            if (currentTile == null) continue;

            // 🔹 Lần 2 cũng check lại hero
            HeroUnit heroOnTile = currentTile.occupyingUnits.Find(u => u is HeroUnit && !u.IsDead) as HeroUnit;
            if (heroOnTile != null)
            {
                heroOnTile.IsDetected = true;
                HeroAlertUI.Instance?.SetDetected(true);

                ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy, heroOnTile));
                Debug.Log($"🚨 {enemy.name} attack {heroOnTile.name} (second wave)");

                continue;
            }

            Vector2Int target = enemy.currentPosition + dir;
            Tile targetTile = GridManager.Instance.GetTileAtPosition(target);
            if (targetTile == null || targetTile.IsObstacle)
                continue;

            int enemyCount = targetTile.occupyingUnits.FindAll(u => u is EnemyUnit).Count;
            if (enemyCount < 4 && targetTile.CanAccept(enemy))
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(target);
                enemy.MoveTo(worldPos, target);
                enemy.OnEnterTile(targetTile);
            }
        }

        yield break;
    }
}
