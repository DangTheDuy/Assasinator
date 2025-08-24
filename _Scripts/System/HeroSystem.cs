using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    private Vector2Int currentDirection;

    private void OnEnable()
    {
        ActionSystem.UnsubscriberReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscriberReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);

        ActionSystem.SubscriberReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscriberReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscriberReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscriberReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public void GenerateNextIntent()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        currentDirection = dirs[Random.Range(0, dirs.Length)];

        IntentUI.Instance.SetDirection(currentDirection);
    }

    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {

    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        EnemyMoveGA moveGA = new EnemyMoveGA { direction = currentDirection };
        ActionSystem.Instance.AddReaction(moveGA);

        GenerateNextIntent();
    }
}
