using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : Singleton<QuestSystem>
{
    private readonly List<Quest> activeQuests = new();

    // Khởi tạo các nhiệm vụ cho màn chơi
    public void InitializeQuests(LevelData levelData)
    {
        ClearCurrentQuests();
        foreach (var questData in levelData.quests)
        {
            Quest newQuest = CreateQuestFromData(questData);
            if (newQuest != null)
            {
                activeQuests.Add(newQuest);
                newQuest.StartQuest();
            }
        }
    }

    private void ClearCurrentQuests()
    {
        foreach (var quest in activeQuests)
        {
            quest.UnsubscribeFromEvents();
            // Optional: Destroy quest objects if they are ScriptableObjects created at runtime
            if (Application.isPlaying)
                Destroy(quest);
        }
        activeQuests.Clear();
    }

    private Quest CreateQuestFromData(QuestData data)
    {
        switch (data.questType)
        {
            case QuestType.EliminateEnemies:
                // Tạo một instance mới của ScriptableObject để tránh ảnh hưởng đến asset gốc
                var elimQuest = ScriptableObject.CreateInstance<EliminateEnemiesQuest>();
                elimQuest.Setup(data.targetEnemyCount, data.targetEnemyName);
                Debug.Log($"[QuestSystem] Tạo nhiệm vụ EliminateEnemies với count = {data.targetEnemyCount}, name = {data.targetEnemyName}");
                return elimQuest;
            case QuestType.MoveToLocation:
                var moveQuest = ScriptableObject.CreateInstance<MoveToLocationQuest>();
                moveQuest.Setup(data.targetPosition);
                return moveQuest;
            default:
                Debug.LogWarning($"Loại nhiệm vụ {data.questType} chưa được hỗ trợ!");
                return null;
        }
    }
}