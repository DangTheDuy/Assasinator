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
        if (enemy.detectedHero == null || enemy.detectedHero.IsDead)
        {
            enemy.SetState(EnemyState.Patrol);
            yield break;
        }

        if (enemy.CanAttack(enemy.detectedHero))
            yield break;

        Vector2Int nextStep = GridManager.Instance.GetStepTowards(enemy.currentPosition, enemy.detectedHero.currentPosition, 2);
        MoveEnemy(enemy, nextStep);
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
        HeroUnit detectedHero = null;
        bool anySeesHero = false;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead || enemy.detectedHero == null) continue;

            if (VisionSystem.Instance.IsTileInVision(enemy, enemy.detectedHero.currentPosition))
            {
                detectedHero = enemy.detectedHero;
                anySeesHero = true;

                if (!enemy.heroVisibleHistory.Contains(detectedHero.currentPosition))
                    enemy.heroVisibleHistory.Add(detectedHero.currentPosition);
            }
        }

        if (anySeesHero && detectedHero != null)
        {
            foreach (var e in allEnemies)
                e?.SetState(EnemyState.Chase, detectedHero);
        }
        else
        {
            foreach (var e in allEnemies)
            {
                if (e == null || e.IsDead) continue;
                e.SetState(e.heroVisibleHistory.Count > 0 ? EnemyState.LostTrack : EnemyState.Patrol);
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
}

// ============================ EXTENSION ============================
    public static class ListExtensions
    {
        public static void AddIfNotContains<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }
    }
