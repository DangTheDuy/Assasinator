
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

        // ======= WAVE 1: thử move =======
        foreach (var enemy in allEnemies)
        {
            // Nếu có hero trong tile → attack luôn, không move
            Tile currentTile = GridManager.Instance.GetTileAtPosition(enemy.currentPosition);
            if (currentTile != null)
            {
                HeroUnit hero = currentTile.occupyingUnits.Find(u => u is HeroUnit) as HeroUnit;
                if (hero != null && !hero.IsDead)
                {
                    ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy, hero));
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
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
                firstWave.Add(enemy);
            }
            else
            {
                secondWave.Add(enemy);
            }

            // 🔹 chờ 1 chút để tạo hiệu ứng di chuyển lần lượt
            yield return new WaitForSeconds(0.2f);
        }

        // ======= WAVE 2: thử lại =======
        foreach (var enemy in secondWave)
        {
            // Kiểm tra lại tile hiện tại, nếu có hero thì attack
            Tile currentTile = GridManager.Instance.GetTileAtPosition(enemy.currentPosition);
            if (currentTile != null)
            {
                HeroUnit hero = currentTile.occupyingUnits.Find(u => u is HeroUnit) as HeroUnit;
                if (hero != null && !hero.IsDead)
                {
                    ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy, hero));
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
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

            yield return new WaitForSeconds(0.2f);
        }
    }
}
