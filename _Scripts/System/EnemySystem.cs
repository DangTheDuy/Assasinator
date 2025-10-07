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
        foreach (var enemy in allEnemies)
        {
            enemy.RegisterHeroMovementListener();
        }
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
        foreach (var enemy in allEnemies)
        {
            enemy.EvaluateVisionForEnemy();
        }

        RefreshEnemies();
        CheckAlertEnd(); // Cập nhật state Chase → LostTrack nếu cần
        UpdateEnemyStateUI();

        // --- Giai đoạn hành động ---
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

                case EnemyState.LostTrack:
                    yield return LostTrackMove(enemy);
                    break;
            }

            yield return new WaitForSeconds(0.2f);
        }

        // --- Sau khi di chuyển, kiểm tra lại xem có thấy hero không ---
        yield return new WaitForSeconds(0.3f);
        EvaluatePostMoveVision();
        UpdateEnemyStateUI();

        // --- Giai đoạn tấn công ---
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

    // --- Cập nhật heroVisibleHistory nếu thấy hero ---
    foreach (var hero in HeroUnit.GetAllHeroes())
    {
        if (hero.IsDead) continue;
        if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
        {
            if (!enemy.heroVisibleHistory.Contains(hero.currentPosition))
                enemy.heroVisibleHistory.Add(hero.currentPosition);
        }
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

private IEnumerator LostTrackMove(EnemyUnit enemy)
{
    if (enemy.heroVisibleHistory.Count == 0)
    {
        enemy.SetState(EnemyState.Patrol);
        yield break;
    }

    // Chọn ô lịch sử xa nhất
    Vector2Int bestTarget = enemy.heroVisibleHistory[0];
    float bestDist = -1f;
    foreach (var trace in enemy.heroVisibleHistory)
    {
        float dist = GridManager.Instance.GetDistance(enemy.currentPosition, trace);
        if (dist > bestDist)
        {
            bestTarget = trace;
            bestDist = dist;
        }
    }

    int steps = Mathf.Min(2, Mathf.CeilToInt(bestDist));

    for (int i = 0; i < steps; i++)
    {
        // Kiểm tra tầm nhìn hero trước khi đi bước tiếp
        HeroUnit seenHero = null;
        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero.IsDead) continue;
            if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
            {
                seenHero = hero;
                if (!enemy.heroVisibleHistory.Contains(hero.currentPosition))
                    enemy.heroVisibleHistory.Add(hero.currentPosition);

                // Chuyển trạng thái Chase ngay, nhưng vẫn đi bước còn lại
                if (enemy.currentState != EnemyState.Chase)
                    enemy.SetState(EnemyState.Chase, hero);
                break;
            }
        }

        // Nếu đã tới ô mục tiêu hoặc không còn bước → dừng
        if (enemy.currentPosition == bestTarget)
            break;

        Vector2Int nextStep = GridManager.Instance.GetStepTowards(enemy.currentPosition, bestTarget, 1);
        Tile targetTile = GridManager.Instance.GetTileAtPosition(nextStep);
        if (targetTile == null || !IsTileAvailableForEnemy(targetTile))
            break;

        Vector3 worldPos = GridManager.Instance.GetWorldPosition(nextStep);
        enemy.MoveTo(worldPos, nextStep);
        enemy.OnEnterTile(targetTile);

        yield return new WaitForSeconds(0.1f);
    }

    // Sau khi đi xong, nếu vẫn chưa thấy hero thì quay lại Patrol
    bool stillSeesHero = false;
    foreach (var hero in HeroUnit.GetAllHeroes())
    {
        if (hero.IsDead) continue;
        if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
        {
            stillSeesHero = true;
            if (!enemy.heroVisibleHistory.Contains(hero.currentPosition))
                enemy.heroVisibleHistory.Add(hero.currentPosition);
            if (enemy.currentState != EnemyState.Chase)
                enemy.SetState(EnemyState.Chase, hero);
            break;
        }
    }

    if (!stillSeesHero && enemy.currentState == EnemyState.LostTrack)
    {
        enemy.SetState(EnemyState.Patrol);
        Debug.Log($"🟢 {enemy.name} không thấy hero → quay lại PATROL");
    }
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
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    // ======================================== ATTACK HANDLER ========================================
    private IEnumerator PerformAttackSafely(EnemyUnit enemy, HeroUnit hero)
    {
        if (enemy == null || hero == null || hero.IsDead) yield break;
        if (isPerformingAction) yield break;
        isPerformingAction = true;
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
        bool anySeesHero = false;
        HeroUnit detectedHero = null;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            if (enemy.detectedHero != null && !enemy.detectedHero.IsDead)
            {
                bool inSight = VisionSystem.Instance.IsTileInVision(enemy, enemy.detectedHero.currentPosition);
                if (inSight)
                {
                    anySeesHero = true;
                    detectedHero = enemy.detectedHero;

                    // ✅ lưu lại vị trí hero đang thấy
                    Vector2Int heroPos = detectedHero.currentPosition;
                    if (!enemy.heroVisibleHistory.Contains(heroPos))
                        enemy.heroVisibleHistory.Add(heroPos);
                }
            }
        }

        if (anySeesHero && detectedHero != null)
        {
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                e.SetState(EnemyState.Chase, detectedHero);
            }
        }
        else
        {
            // Không ai thấy hero → chuyển LostTrack
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                if (e.heroVisibleHistory.Count > 0)
                    e.SetState(EnemyState.LostTrack);
                else
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
        int patrolCount = 0, chaseCount = 0, lostTrack = 0;

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
                case EnemyState.LostTrack:
                    lostTrack++;
                    break;
            }
        }

        stateSummary = $" Patrol: {patrolCount}\n" + $" LostTrack: {lostTrack}\n" +
                    $" Chase: {chaseCount}";

        enemyStateText.text = stateSummary;
    }

    private void EvaluatePostMoveVision()
{
    HeroUnit heroToChase = null;

    foreach (var enemy in allEnemies)
    {
        if (enemy == null || enemy.IsDead) continue;

        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero.IsDead) continue;

            if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
            {
                heroToChase = hero;
                break;
            }
        }

        if (heroToChase != null) break;
    }

    if (heroToChase != null)
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            enemy.SetState(EnemyState.Chase, heroToChase);
        }
        Debug.Log($"🔁 GLOBAL RE-CHASE: hero {heroToChase.name} được nhìn thấy → tất cả Chase");
    }
    else
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.heroVisibleHistory.Count > 0)
                enemy.SetState(EnemyState.LostTrack);
            else
                enemy.SetState(EnemyState.Patrol);
        }
        Debug.Log("🟢 GLOBAL RESET: không ai thấy hero → Patrol hoặc LostTrack");
    }
}


    private void EvaluateVisionForEnemy(EnemyUnit enemy)
{
    foreach (var hero in HeroUnit.GetAllHeroes())
    {
        if (hero.IsDead) continue;

        // Kiểm tra nếu hero trong tầm nhìn
        if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
        {
            if (!enemy.heroVisibleHistory.Contains(hero.currentPosition))
                enemy.heroVisibleHistory.Add(hero.currentPosition);

            
        }
    }
}


}
