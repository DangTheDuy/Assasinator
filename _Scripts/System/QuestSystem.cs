using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : Singleton<QuestSystem>
{
    private readonly List<Quest> activeQuests = new();
    private List<QuestData> allQuests;
    private int currentQuestIndex = -1;

    public void InitializeQuests(LevelData levelData)
    {
        ClearCurrentQuests();
        allQuests = levelData.quests;
        currentQuestIndex = -1;

        if (levelData.isSequential)
        {
            LoadNextQuest();
        }
        else
        {
            LoadAllQuestsInParallel();
        }
    }

    private void LoadNextQuest()
    {
        currentQuestIndex++;
        if (currentQuestIndex < allQuests.Count)
        {
            QuestData nextQuestData = allQuests[currentQuestIndex];
            Quest newQuest = CreateQuestFromData(nextQuestData);
            if (newQuest != null)
            {
                activeQuests.Add(newQuest);
                newQuest.StartQuest();
                newQuest.OnQuestCompleted += HandleSequentialQuestCompleted;
                Debug.Log($"[QuestSystem]nhiệm vụ mới: {newQuest.questName}");
            }
        }
        else
        {
            Debug.Log("[QuestSystem] Tất cả nhiệm vụ đã hoàn thành!");
            // Kích hoạt logic kết thúc level hoặc màn hình chiến thắng ở đây
        }
    }

    private void LoadAllQuestsInParallel()
    {
        foreach (var questData in allQuests)
        {
            Quest newQuest = CreateQuestFromData(questData);
            if (newQuest != null)
            {
                activeQuests.Add(newQuest);
                newQuest.StartQuest();
                newQuest.OnQuestCompleted += HandleParallelQuestCompleted;
                Debug.Log($"[QuestSystem] Bắt đầu nhiệm vụ song song: {newQuest.questName}");
            }
        }
    }

    private void HandleSequentialQuestCompleted()
    {
        // Gỡ bỏ nhiệm vụ vừa hoàn thành
        activeQuests[0].OnQuestCompleted -= HandleSequentialQuestCompleted;
        Destroy(activeQuests[0]);
        activeQuests.RemoveAt(0);
        
        Debug.Log("[QuestSystem] Hoàn thành nhiệm vụ hiện tại, chuyển sang nhiệm vụ tiếp theo...");
        LoadNextQuest();
    }

    private void HandleParallelQuestCompleted()
    {
        // Kiểm tra xem tất cả nhiệm vụ song song đã hoàn thành chưa
        int completedCount = 0;
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            if (activeQuests[i].state == QuestState.Completed)
            {
                activeQuests[i].OnQuestCompleted -= HandleParallelQuestCompleted;
                Destroy(activeQuests[i]);
                activeQuests.RemoveAt(i);
                completedCount++;
            }
        }
        
        if (activeQuests.Count == 0)
        {
            Debug.Log("[QuestSystem] Tất cả nhiệm vụ song song đã hoàn thành!");
            // Kích hoạt logic kết thúc level hoặc màn hình chiến thắng ở đây
        }
        else
        {
            Debug.Log($"[QuestSystem] Hoàn thành {completedCount} nhiệm vụ. Còn lại: {activeQuests.Count}");
        }
    }
    
    private void ClearCurrentQuests()
    {
        foreach (var quest in activeQuests)
        {
            quest.UnsubscribeFromEvents();
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
                var elimQuest = ScriptableObject.CreateInstance<EliminateEnemiesQuest>();
                elimQuest.Setup(data.targetEnemyCount, data.targetEnemyName);
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