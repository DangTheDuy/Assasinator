using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    private readonly List<EnemyUnit> allEnemies = new();
    private bool isAlert = false;
    private bool isPerformingAction = false;

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

    // ======================================== TURN LOGIC ========================================
    public IEnumerator PerformEnemyTurn()
    {
        RefreshEnemies();

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

                case EnemyState.Alert:
                    // không di chuyển, chỉ gọi đồng đội
                    break;
            }

            yield return new WaitForSeconds(0.2f);
        }

        // --- Phase 2: ATTACK ALL ---
        yield return PerformAllAttacks();
        CheckAlertEnd();
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

        isPerformingAction = true; // 🔒 khóa toàn cục
        Debug.Log($"⚔️ {enemy.name} tấn công {hero.name}");

        yield return ActionSystem.Instance.PerformAndWait(new AttackHeroGA(enemy, hero));

        isPerformingAction = false; // 🔓 mở khóa
    }

    // ======================================== ALERT MANAGEMENT ========================================
    public void TriggerAlert(EnemyUnit source)
    {
        if (source == null || source.detectedHero == null) return;

        isAlert = true;
        HeroUnit hero = source.detectedHero;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            // Nếu enemy cùng tile hoặc xung quanh tile phát hiện → cũng gán target hero này
            int dist = GridManager.Instance.GetDistance(enemy.currentPosition, source.currentPosition);
            if (dist <= 1)
            {
                enemy.SetState(EnemyState.Chase, hero);
            }
            else
            {
                // Các enemy khác vẫn patrol bình thường
                if (enemy.currentState != EnemyState.Chase)
                    enemy.SetState(EnemyState.Patrol);
            }
        }

        Debug.Log($"🚨 Báo động! Enemy xung quanh {source.name} truy đuổi {hero.name}");
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

            // ✅ kiểm tra hero còn trong tầm nhìn
            if (enemy.detectedHero != null && !enemy.detectedHero.IsDead)
            {
                int dist = GridManager.Instance.GetDistance(enemy.currentPosition, enemy.detectedHero.currentPosition);
                if (dist <= enemy.visionRange)
                {
                    heroVisibleToAny = true;
                    break;
                }
            }
        }

        // ❌ không còn ai thấy hero hoặc không còn ai chase → hết cảnh báo
        if (!anyChasing || !heroVisibleToAny)
        {
            isAlert = false;
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                e.SetState(EnemyState.Patrol);
            }

            Debug.Log("🔔 Hero đã thoát khỏi tầm nhìn, tất cả enemy quay lại tuần tra.");
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

}
