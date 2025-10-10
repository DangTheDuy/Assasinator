// File: QuestSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : Singleton<QuestSystem>
{
    private readonly List<Quest> activeQuests = new();
    private List<QuestData> allQuests;
    private int currentQuestIndex = -1;
    private bool isSequential;

    public static event Action<Quest> OnQuestLoaded;
    public static event Action<Quest> OnQuestCompleted;

    public void InitializeQuests(LevelData levelData)
    {
        ClearCurrentQuests();
        allQuests = levelData.quests;
        currentQuestIndex = -1;
        isSequential = levelData.isSequential;

        if (isSequential)
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
                // Gán phương thức callback chung
                newQuest.OnCompletedCallback = HandleQuestCompleted; 
                newQuest.StartQuest();
                OnQuestLoaded?.Invoke(newQuest);
                Debug.Log($"[QuestSystem] Bắt đầu nhiệm vụ mới: {newQuest.questName}");
            }
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
                // Gán phương thức callback chung
                newQuest.OnCompletedCallback = HandleQuestCompleted; 
                newQuest.StartQuest();
                OnQuestLoaded?.Invoke(newQuest);
                Debug.Log($"[QuestSystem] Bắt đầu nhiệm vụ song song: {newQuest.questName}");
            }
        }
    }

    private void HandleQuestCompleted(Quest completedQuest)
    {
        // Phát ra sự kiện hoàn thành để UIManager lắng nghe
        OnQuestCompleted?.Invoke(completedQuest);

        if (isSequential)
        {
            // Xử lý logic cho nhiệm vụ nối tiếp
            if (activeQuests.Count > 0 && activeQuests[0] == completedQuest)
            {
                activeQuests.RemoveAt(0);
                Destroy(completedQuest);
                LoadNextQuest();
            }
        }
        else
        {
            // Xử lý logic cho nhiệm vụ song song
            activeQuests.Remove(completedQuest);
            Destroy(completedQuest);
            
            if (activeQuests.Count == 0)
            {
                Debug.Log("[QuestSystem] Tất cả nhiệm vụ song song đã hoàn thành!");
            }
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