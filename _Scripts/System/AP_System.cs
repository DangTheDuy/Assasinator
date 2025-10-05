using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AP_System : Singleton<AP_System>
{
    void OnEnable()
    {
        ActionSystem.SubscriberReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.UnsubscriberReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    // Hồi AP cho tất cả Hero vào đầu lượt
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        TurnManager.Instance.EndEnemyTurn();
    }

    public void RefillAllHeroes()
    {
        foreach (HeroUnit hero in Unit.AllUnits.OfType<HeroUnit>())
        {
            hero.RefillAP();
        }

    }

    // Buff AP cho toàn đội
   /* public void GainAPForAll(int amount)
    {
        foreach (HeroUnit hero in Unit.AllUnits)
        {
            if (hero != null)
                hero.GainAP(amount);
        }
    }

    // Truy cập thống kê AP toàn đội
    public Dictionary<HeroUnit, int> GetAllHeroAP()
    {
        Dictionary<HeroUnit, int> result = new();
        foreach (HeroUnit hero in Unit.AllUnits)
        {
            if (hero != null)
                result[hero] = hero.CurrentAP;
        }
        return result;
    }*/
}
