using UnityEngine;

public class AttackHeroGA : GameAction
{
    public EnemyUnit attacker;
    public HeroUnit target;

    public AttackHeroGA(EnemyUnit attacker, HeroUnit target)
    {
        this.attacker = attacker;
        this.target = target;
    }
}
