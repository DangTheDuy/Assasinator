using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    private readonly List<EnemyUnit> allEnemies = new();
    private bool isPerformingAction = false;

    [Header("UI Tracking")]
    public TMP_Text enemyStateText;

    // ============================ INIT ============================
    private void Start()
    {
        RefreshEnemies();
    }

    public void RefreshEnemies()
    {
        allEnemies.Clear();
        allEnemies.AddRange(FindObjectsOfType<EnemyUnit>());
        foreach (var enemy in allEnemies)
            enemy.RegisterHeroMovementListener();
    }

    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (enemy != null && !allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyUnit enemy)
    {
        if (enemy == null) return;

        if (allEnemies.Contains(enemy))
        {
            allEnemies.Remove(enemy);
        }
        UpdateEnemyStateUI();
    }


    public List<EnemyUnit> GetAllEnemies() => allEnemies;

    public bool IsAnyEnemyChasing()
    {
        foreach (var e in allEnemies)
            if (e != null && !e.IsDead && e.currentState == EnemyState.Chase)
                return true;
        return false;
    }

    // ============================ TURN LOGIC ============================
    public IEnumerator PerformEnemyTurn()
    {
        UpdateVisionForAllEnemies();
        RefreshEnemies();
        CheckAlertEnd();
        UpdateEnemyStateUI();

        // --- Movement Phase ---
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            yield return enemy.currentState switch
            {
                EnemyState.Patrol => PatrolMove(enemy),
                EnemyState.Chase => ChaseMove(enemy),
                EnemyState.LostTrack => LostTrackMove(enemy),
                _ => null
            };

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.3f);

        EvaluatePostMoveVision();
        UpdateEnemyStateUI();

        // --- Attack Phase ---
        yield return PerformAllAttacks();

        TurnManager.Instance.EndEnemyTurn();
    }

    // ============================ MOVEMENT ============================
    private IEnumerator PatrolMove(EnemyUnit enemy)
    {
        MoveEnemy(enemy, HeroSystem.Instance.GetCurrentIntentDirection(), 1);
        UpdateEnemyVision(enemy);
        yield return null;
    }

    private IEnumerator ChaseMove(EnemyUnit enemy)
    {
        // 1️⃣ Nếu không có hero để theo đuổi → tìm hero gần nhất có thể thấy
        HeroUnit targetHero = null;

        // Ưu tiên hero trong tầm nhìn
        targetHero = FindNearestVisibleHero(enemy);

        // Nếu không có hero nào thấy trực tiếp → giữ hero cũ (nếu còn) hoặc chọn hero gần nhất tổng thể
        if (targetHero == null)
        {
            if (enemy.detectedHero != null && !enemy.detectedHero.IsDead)
            {
                targetHero = enemy.detectedHero;
            }
            else
            {
                targetHero = FindNearestHeroByDistance(enemy);
            }
        }

        // Nếu vẫn không có hero → quay về tuần tra
        if (targetHero == null)
        {
            enemy.SetState(EnemyState.Patrol);
            yield break;
        }

        // Cập nhật trạng thái nếu khác
        if (enemy.detectedHero != targetHero)
        {
            enemy.SetState(EnemyState.Chase, targetHero);
            Debug.Log($"🎯 {enemy.name} chọn mục tiêu mới: {targetHero.name}");
        }

        // 2️⃣ Nếu đang trong tầm tấn công → không cần di chuyển
        if (enemy.CanAttack(targetHero))
            yield break;

        // 3️⃣ Tìm đường di chuyển tới hero gần nhất
        Vector2Int nextStep = GridManager.Instance.GetStepTowards(enemy.currentPosition, targetHero.currentPosition, 2);
        Tile targetTile = GridManager.Instance.GetTileAtPosition(nextStep);

        // Nếu tile chính không khả dụng → tìm ô gần mục tiêu mà còn chỗ cho enemy
        if (targetTile == null || !IsTileAvailableForEnemy(targetTile))
        {
            Vector2Int altTarget = GridManager.Instance.GetNearestAvailableTilePosition(targetHero.currentPosition, enemy.currentPosition, 6);
            if (altTarget == Vector2Int.zero)
            {
                Debug.Log($"🚫 {enemy.name} không tìm được ô thay thế gần {targetHero.name}");
                yield break;
            }

            // Tính bước đi hướng tới ô thay thế (vẫn dùng số bước phù hợp, ví dụ 2)
            nextStep = GridManager.Instance.GetStepTowards(enemy.currentPosition, altTarget, 2);
            targetTile = GridManager.Instance.GetTileAtPosition(nextStep);

            if (targetTile == null || !IsTileAvailableForEnemy(targetTile))
            {
                // an toàn: nếu ô nextStep vẫn ko hợp lệ thì abort
                Debug.Log($"⚠️ {enemy.name} bước kế tiếp {nextStep} vẫn không khả dụng.");
                yield break;
            }
        }


        // 5️⃣ Di chuyển
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(nextStep);
        enemy.MoveTo(worldPos, nextStep);
        enemy.OnEnterTile(targetTile);

        // 6️⃣ Cập nhật dấu vết
        enemy.heroVisibleHistory.AddIfNotContains(targetHero.currentPosition);

        // 7️⃣ Sau khi di chuyển, kiểm tra lại tầm nhìn
        if (VisionSystem.Instance.IsTileInVision(enemy, targetHero.currentPosition))
        {
            enemy.SetState(EnemyState.Chase, targetHero);
        }
        else
        {
            enemy.SetState(EnemyState.LostTrack, targetHero);
        }

        yield return null;
    }

    private IEnumerator LostTrackMove(EnemyUnit enemy)
    {
        if (enemy.heroVisibleHistory.Count == 0)
        {
            enemy.SetState(EnemyState.Patrol);
            yield break;
        }

        Vector2Int target = GetFurthestHeroPosition(enemy);
        int steps = Mathf.Min(2, Mathf.CeilToInt(GridManager.Instance.GetDistance(enemy.currentPosition, target)));

        for (int i = 0; i < steps; i++)
        {
            HeroUnit seenHero = CheckVisionDuringMove(enemy);
            if (seenHero != null) break;

            Vector2Int nextStep = GridManager.Instance.GetStepTowards(enemy.currentPosition, target, 1);
            if (!MoveEnemy(enemy, nextStep))
                break;

            yield return new WaitForSeconds(0.1f);
        }

        if (enemy.currentState == EnemyState.LostTrack && CheckVisionDuringMove(enemy) == null)
            enemy.SetState(EnemyState.Patrol);
    }

    // ============================ ATTACK ============================
    private IEnumerator PerformAllAttacks()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            // ✅ chỉ attack nếu enemy thật sự đang chiếm một tile hợp lệ
            Tile tile = GridManager.Instance.GetTileAtPosition(enemy.currentPosition);
            if (tile == null || tile.IsObstacle || !tile.occupyingUnits.Contains(enemy))
                continue;

            HeroUnit target = GetAttackableHero(enemy);
            if (target != null)
            {
                yield return PerformAttackSafely(enemy, target);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }


    private IEnumerator PerformAttackSafely(EnemyUnit enemy, HeroUnit hero)
    {
        if (enemy == null || hero == null || hero.IsDead || isPerformingAction)
            yield break;

        isPerformingAction = true;
        yield return ActionSystem.Instance.PerformAndWait(new AttackHeroGA(enemy, hero));
        isPerformingAction = false;
    }

    // ============================ ALERT MANAGEMENT ============================
    public void TriggerGlobalChase(HeroUnit hero)
    {
        if (hero == null) return;

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
        HeroUnit lastSeenHero = null;
        Vector2Int lastSeenPos = Vector2Int.zero;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            HeroUnit visibleHero = FindNearestVisibleHero(enemy);
            if (visibleHero != null)
            {
                anySeesHero = true;
                lastSeenHero = visibleHero;
                lastSeenPos = visibleHero.currentPosition;

                // Nếu enemy đang theo dõi hero khác → chuyển sang hero mới
                if (enemy.detectedHero != visibleHero)
                {
                    enemy.SetState(EnemyState.Chase, visibleHero);
                    Debug.Log($"👀 {enemy.name} chuyển mục tiêu sang {visibleHero.name}");
                }
                enemy.heroVisibleHistory.AddIfNotContains(visibleHero.currentPosition);
            }
        }

        if (anySeesHero && lastSeenHero != null)
        {
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                e.heroVisibleHistory.AddIfNotContains(lastSeenPos);
                e.SetState(EnemyState.Chase, lastSeenHero);
            }
        }
        else
        {
            // ❔ Không ai thấy hero nào → tất cả LostTrack
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                if (e.heroVisibleHistory.Count > 0)
                {
                    Vector2Int furthest = GetFurthestHeroPosition(e);
                    e.heroVisibleHistory.AddIfNotContains(furthest);
                    e.SetState(EnemyState.LostTrack, e.detectedHero);
                }
                else
                {
                    e.SetState(EnemyState.Patrol);
                }
            }
        }
    }

    // ============================ HELPERS ============================
    private void UpdateVisionForAllEnemies()
    {
        foreach (var enemy in allEnemies)
            UpdateEnemyVision(enemy);
    }

    private void UpdateEnemyVision(EnemyUnit enemy)
    {
        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero.IsDead) continue;
            if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
                enemy.heroVisibleHistory.AddIfNotContains(hero.currentPosition);
        }
    }

    private HeroUnit GetAttackableHero(EnemyUnit enemy)
    {
        foreach (var unit in Unit.AllUnits)
        {
            if (unit is HeroUnit hero && !hero.IsDead)
            {
                int dist = GridManager.Instance.GetDistance(enemy.currentPosition, hero.currentPosition);
                if (dist <= enemy.AttackRange) return hero;
            }
        }
        return null;
    }

    private Vector2Int GetFurthestHeroPosition(EnemyUnit enemy)
    {
        Vector2Int best = enemy.heroVisibleHistory[0];
        float maxDist = -1f;

        foreach (var pos in enemy.heroVisibleHistory)
        {
            float d = GridManager.Instance.GetDistance(enemy.currentPosition, pos);
            if (d > maxDist)
            {
                maxDist = d;
                best = pos;
            }
        }
        return best;
    }

    private HeroUnit CheckVisionDuringMove(EnemyUnit enemy)
    {
        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero.IsDead) continue;
            if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
            {
                enemy.heroVisibleHistory.AddIfNotContains(hero.currentPosition);
                if (enemy.currentState != EnemyState.Chase)
                    enemy.SetState(EnemyState.Chase, hero);
                return hero;
            }
        }
        return null;
    }

    private bool MoveEnemy(EnemyUnit enemy, Vector2Int direction, int distance = 1)
    {
        Vector2Int nextPos = enemy.currentPosition + direction;
        return MoveEnemy(enemy, nextPos);
    }

    private bool MoveEnemy(EnemyUnit enemy, Vector2Int nextPos)
    {
        Tile tile = GridManager.Instance.GetTileAtPosition(nextPos);
        if (tile == null || !IsTileAvailableForEnemy(tile)) return false;

        enemy.MoveTo(GridManager.Instance.GetWorldPosition(nextPos), nextPos);
        enemy.OnEnterTile(tile);
        return true;
    }

    private bool IsTileAvailableForEnemy(Tile tile)
    {
        return tile != null && !tile.IsObstacle && tile.occupyingUnits.FindAll(u => u is EnemyUnit).Count < 4;
    }

    public void UpdateEnemyStateUI()
    {
        if (enemyStateText == null) return;

        int patrol = 0, chase = 0, lost = 0;
        foreach (var e in allEnemies)
        {
            if (e == null || e.IsDead) continue;
            switch (e.currentState)
            {
                case EnemyState.Patrol: patrol++; break;
                case EnemyState.Chase: chase++; break;
                case EnemyState.LostTrack: lost++; break;
            }
        }

        enemyStateText.text = $"Patrol: {patrol}\nLostTrack: {lost}\nChase: {chase}";
    }

    private void EvaluatePostMoveVision()
    {
        HeroUnit heroToChase = null;
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            heroToChase = CheckVisionDuringMove(enemy);
            if (heroToChase != null) break;
        }

        foreach (var e in allEnemies)
        {
            if (e == null || e.IsDead) continue;
            if (heroToChase != null)
                e.SetState(EnemyState.Chase, heroToChase);
            else
                e.SetState(e.heroVisibleHistory.Count > 0 ? EnemyState.LostTrack : EnemyState.Patrol);
        }
    }
    
    private HeroUnit FindNearestVisibleHero(EnemyUnit enemy, HeroUnit exclude = null)
    {
        HeroUnit best = null;
        float bestDist = float.MaxValue;

        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero == null || hero.IsDead || hero == exclude) continue;

            if (VisionSystem.Instance.IsTileInVision(enemy, hero.currentPosition))
            {
                float dist = GridManager.Instance.GetDistance(enemy.currentPosition, hero.currentPosition);
                if (dist < bestDist)
                {
                    best = hero;
                    bestDist = dist;
                }
            }
        }
        return best;
    }

    private HeroUnit FindNearestHeroByDistance(EnemyUnit enemy)
    {
        HeroUnit best = null;
        float bestDist = float.MaxValue;

        foreach (var hero in HeroUnit.GetAllHeroes())
        {
            if (hero == null || hero.IsDead) continue;
            float dist = GridManager.Instance.GetDistance(enemy.currentPosition, hero.currentPosition);
            if (dist < bestDist)
            {
                best = hero;
                bestDist = dist;
            }
        }
        return best;
    }

    public void UpdateEnemyVisibility()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            Tile tile = GridManager.Instance.GetTileAtPosition(enemy.currentPosition);
            enemy.SetVisibility(tile != null && tile.visibleCount > 0);
        }
    }

}

// ============================ EXTENSION ============================
    public static class ListExtensions
    {
        public static void AddIfNotContains<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }
    }

    
