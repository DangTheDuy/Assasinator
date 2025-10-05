using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    private Vector2Int currentDirection;

    private void Start()
    {
        GenerateNextIntent();
    }

    public List<HeroUnit> GetAllHeroes()
    {
        return new List<HeroUnit>(FindObjectsOfType<HeroUnit>());
    }
    public void GenerateNextIntent()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        currentDirection = dirs[Random.Range(0, dirs.Length)];
        IntentUI.Instance?.SetDirection(currentDirection);
    }

    public void EndHeroTurn()
    {
        TurnManager.Instance.EndHeroTurn();
    }

    public void OnHeroTurnStart()
    {
        foreach (var hero in GetAllHeroes())
        {
            if (hero == null || hero.IsDead) continue;
            hero.RefillAP();
            hero.UpdateHUD();
        }

        GenerateNextIntent();
    }

    private void OnEnable()
    {
        TurnManager.OnTurnStart += HandleTurnStart;
    }

    private void OnDisable()
    {
        TurnManager.OnTurnStart -= HandleTurnStart;
    }

    private void HandleTurnStart(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.Hero)
            OnHeroTurnStart();
    }
    
    public Vector2Int GetCurrentIntentDirection()
    {
        return currentDirection;
    }
}
