// File: Quest.cs
using System;
using UnityEngine;

// Định nghĩa trạng thái của nhiệm vụ
public enum QuestState
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public abstract class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questName;
    [TextArea(3, 5)]
    public string description;

    public QuestState state { get; protected set; } = QuestState.NotStarted;

    public event Action OnQuestCompleted;
    public event Action OnQuestFailed;
    
    // Khởi tạo nhiệm vụ
    public virtual void StartQuest()
    {
        state = QuestState.InProgress;
        SubscribeToEvents();
        Debug.Log($"Nhiệm vụ '{questName}' đã bắt đầu!");
    }

    // Kiểm tra tiến độ và điều kiện hoàn thành
    public abstract void OnUpdate();

    // Hoàn thành nhiệm vụ
    protected void CompleteQuest()
    {
        if (state != QuestState.InProgress) return;
        state = QuestState.Completed;
        UnsubscribeFromEvents();
        OnQuestCompleted?.Invoke();
        Debug.Log($"Nhiệm vụ '{questName}' đã hoàn thành!");
    }

    // Nhiệm vụ thất bại
    protected void FailQuest()
    {
        if (state != QuestState.InProgress) return;
        state = QuestState.Failed;
        UnsubscribeFromEvents();
        OnQuestFailed?.Invoke();
        Debug.Log($"Nhiệm vụ '{questName}' đã thất bại!");
    }

    // Đăng ký các sự kiện cần theo dõi
    public abstract void SubscribeToEvents();

    // Hủy đăng ký khi nhiệm vụ kết thúc
    public abstract void UnsubscribeFromEvents();
}