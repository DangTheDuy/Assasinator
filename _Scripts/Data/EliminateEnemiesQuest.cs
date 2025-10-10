// File: EliminateEnemiesQuest.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Eliminate Enemies Quest", menuName = "Quests/Eliminate Enemies")]
public class EliminateEnemiesQuest : Quest
{
    private int targetCount;
    private int currentKills = 0;
    private string targetName;
    public int CurrentKills => currentKills;
    public int TargetCount => targetCount;

    public void Setup(int count, string name)
    {
        targetCount = count;
        targetName = name;
        questName = string.IsNullOrEmpty(targetName) ? $"Tiêu diệt {targetCount} kẻ địch" : $"Tiêu diệt '{targetName}'";
        description = string.IsNullOrEmpty(targetName) ? $"Hãy tiêu diệt {targetCount} kẻ địch trên bản đồ." : $"Hãy tìm và tiêu diệt mục tiêu chính: {targetName}.";
    }

    public override void SubscribeToEvents()
    {
        ActionSystem.SubscriberReaction<UnitDiedGA>(OnUnitDied, ReactionTiming.POST);
    }

    public override void UnsubscribeFromEvents()
    {
        ActionSystem.UnsubscriberReaction<UnitDiedGA>(OnUnitDied, ReactionTiming.POST);
    }

    public override void OnUpdate()
    {
        // Logic cập nhật có thể phức tạp hơn nếu cần
    }

    private void OnUnitDied(UnitDiedGA action)
    {
        if (action.deadUnit is EnemyUnit)
        {
            if (string.IsNullOrEmpty(targetName) || action.deadUnit.name == targetName)
            {
                currentKills++;
                OnQuestProgressUpdated?.Invoke(this); 
                Debug.Log($"diệt {currentKills}/{targetCount}");
                if (currentKills >= targetCount)
                {
                    CompleteQuest();
                }
            }
        }
    }
}