using System;
using System.Collections;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    public enum TurnPhase { Hero, Enemy }
    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Hero;
    public int CurrentTurn { get; private set; } = 1;

    public static event Action<TurnPhase> OnTurnStart;
    public static event Action<TurnPhase> OnTurnEnd;
    public static event Action<int> OnNewTurnStarted;


    public void EndHeroTurn()
    {
        if (CurrentPhase != TurnPhase.Hero) return;

        Debug.Log($"[TurnManager] Hero Turn {CurrentTurn} ended → Enemy Turn begins");
        OnTurnEnd?.Invoke(TurnPhase.Hero);

        CurrentPhase = TurnPhase.Enemy;
        OnTurnStart?.Invoke(TurnPhase.Enemy);

        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        yield return EnemySystem.Instance.PerformEnemyTurn();
        EndEnemyTurn(); 
    }

    public void EndEnemyTurn()
    {
        if (CurrentPhase != TurnPhase.Enemy) return;

        Debug.Log($"Turn {CurrentTurn + 1} begins");
        OnTurnEnd?.Invoke(TurnPhase.Enemy);
        CurrentTurn++;
        OnNewTurnStarted?.Invoke(CurrentTurn);

        AP_System.Instance.RefillAllHeroes();

        CurrentPhase = TurnPhase.Hero;
        OnTurnStart?.Invoke(TurnPhase.Hero);
        foreach (HeroUnit hero in FindObjectsOfType<HeroUnit>())
        {
            hero.OnTurnStart();
        }
    }
}
