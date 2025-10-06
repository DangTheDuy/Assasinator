using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemySystem : Singleton<EnemySystem>
{
    private readonly List<EnemyUnit> allEnemies = new();
    private bool isPerformingAction = false;
    [Header("UI Tracking")]
    public TMP_Text enemyStateText;


    // ======================================== INIT ========================================
    private void Start()
    {
        RefreshEnemies();
    }

    public void RefreshEnemies()
    {
        allEnemies.Clear();
        allEnemies.AddRange(FindObjectsOfType<EnemyUnit>());
    }

    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (enemy == null) return;
        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
        }
    }

    // ======================================== TURN LOGIC ========================================
    public IEnumerator PerformEnemyTurn()
    {
        RefreshEnemies();
        CheckAlertEnd();
        UpdateEnemyStateUI();
        // --- Phase 1: MOVE ALL ---
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            switch (enemy.currentState)
            {
                case EnemyState.Patrol:
                    yield return PatrolMove(enemy);
                    break;
                case EnemyState.Chase:
                    yield return ChaseMove(enemy);
                    break;
            }
            yield return new WaitForSeconds(0.2f);
        }
        // --- Phase 2: ATTACK ALL ---
        yield return PerformAllAttacks();
        TurnManager.Instance.EndEnemyTurn();
    }

    // ======================================== STATE: PATROL ========================================
    private IEnumerator PatrolMove(EnemyUnit enemy)
    {
        Vector2Int dir = HeroSystem.Instance.GetCurrentIntentDirection();
        Vector2Int nextPos = enemy.currentPosition + dir;
        Tile targetTile = GridManager.Instance.GetTileAtPosition(nextPos);

        if (targetTile != null && targetTile.CanAccept(enemy) && !targetTile.IsObstacle)
        {
            Vector3 worldPos = GridManager.Instance.GetWorldPosition(nextPos);
            enemy.MoveTo(worldPos, nextPos);
            enemy.OnEnterTile(targetTile);
        }

        yield return null;
    }

    // ======================================== STATE: CHASE ========================================
    private IEnumerator ChaseMove(EnemyUnit enemy)
    {
        if (enemy.detectedHero == null || enemy.detectedHero.IsDead)
        {
            enemy.SetState(EnemyState.Patrol);
            yield break;
        }

        HeroUnit targetHero = enemy.detectedHero;

        // Nếu trong tầm thì không cần move
        if (enemy.CanAttack(targetHero))
            yield break;

        Vector2Int heroPos = targetHero.currentPosition;
        Vector2Int nextStep = GridManager.Instance.GetStepTowards(enemy.currentPosition, heroPos, 2);
        Tile targetTile = GridManager.Instance.GetTileAtPosition(nextStep);

        // Nếu tile chính không khả dụng → thử chọn tile lân cận
        if (targetTile == null || !IsTileAvailableForEnemy(targetTile))
        {
            Tile alt = FindNearestAvailableTile(enemy);
            if (alt != null)
            {
                nextStep = alt.gridPosition;
                targetTile = alt;
            }
            else
            {
                Debug.Log($" {enemy.name} không có tile trống để di chuyển, giữ nguyên vị trí.");
                yield break;
            }
        }

        Vector3 worldPos = GridManager.Instance.GetWorldPosition(nextStep);
        enemy.MoveTo(worldPos, nextStep);
        enemy.OnEnterTile(targetTile);

        yield return null;
        CheckAlertEnd();
    }


    // ======================================== PHASE: ATTACK ALL ========================================
    private IEnumerator PerformAllAttacks()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            HeroUnit target = GetAttackableHero(enemy);
            if (target != null)
            {
                yield return PerformAttackSafely(enemy, target);
                yield return new WaitForSeconds(0.2f); // delay nhẹ giữa các enemy attack
            }
        }
    }

    // ======================================== ATTACK HANDLER ========================================
    private IEnumerator PerformAttackSafely(EnemyUnit enemy, HeroUnit hero)
    {
        if (enemy == null || hero == null || hero.IsDead) yield break;
        if (isPerformingAction) yield break;

        isPerformingAction = true;
        Debug.Log($"⚔️ {enemy.name} tấn công {hero.name}");

        yield return ActionSystem.Instance.PerformAndWait(new AttackHeroGA(enemy, hero));

        isPerformingAction = false;
    }

    // ======================================== ALERT MANAGEMENT ========================================

    public void TriggerGlobalChase(HeroUnit hero)
    {
        if (hero == null) return;
        Debug.Log($"⚡ GLOBAL CHASE: Tất cả enemy truy đuổi {hero.name}");

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            enemy.SetState(EnemyState.Chase, hero);
        }
        UpdateEnemyStateUI(); 
    }

    public void CheckAlertEnd()
    {
        bool anyChasing = false;
        bool heroVisibleToAny = false;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.currentState != EnemyState.Chase) continue;
            anyChasing = true;
            if (enemy.detectedHero != null && !enemy.detectedHero.IsDead)
            {
                if (VisionSystem.Instance.IsTileInVision(enemy, enemy.detectedHero.currentPosition))
                {
                    heroVisibleToAny = true;
                    break;
                }
            }
        }
        if (!anyChasing || !heroVisibleToAny)
        {
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                e.SetState(EnemyState.Patrol);
            }
        }
    }
    // ======================================== HELPERS ========================================
    private HeroUnit GetAttackableHero(EnemyUnit enemy)
    {
        foreach (var unit in Unit.AllUnits)
        {
            if (unit is HeroUnit hero && !hero.IsDead)
            {
                int dist = GridManager.Instance.GetDistance(enemy.currentPosition, hero.currentPosition);
                if (dist <= enemy.AttackRange)
                    return hero;
            }
        }
        return null;
    }

    private bool IsTileAvailableForEnemy(Tile tile)
    {
        if (tile == null || tile.IsObstacle) return false;
        int enemyCount = tile.occupyingUnits.FindAll(u => u is EnemyUnit).Count;
        return enemyCount < 4;
    }

    private Tile FindNearestAvailableTile(EnemyUnit enemy)
    {
        // tìm tile gần hero nhất nhưng còn slot trống
        Vector2Int pos = enemy.currentPosition;
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        };

        foreach (var n in neighbors)
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(n);
            if (IsTileAvailableForEnemy(tile))
                return tile;
        }
        return null;
    }

    public List<EnemyUnit> GetAllEnemies()
    {
        return allEnemies;
    }

    public bool IsAnyEnemyChasing()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.currentState == EnemyState.Chase)
                return true;
        }
        return false;
    }
    
    public void UpdateEnemyStateUI()
    {
        if (enemyStateText == null) return;

        string stateSummary = "";
        int patrolCount = 0,  chaseCount = 0;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            switch (enemy.currentState)
            {
                case EnemyState.Patrol:
                    patrolCount++;
                    break;
                case EnemyState.Chase:
                    chaseCount++;
                    break;
            }
        }

        stateSummary = $" Patrol: {patrolCount}\n" +
                    $" Chase: {chaseCount}";

        enemyStateText.text = stateSummary;
    }

}
