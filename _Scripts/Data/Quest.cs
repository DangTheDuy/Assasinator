// File: Quest.cs
using System;
using UnityEngine;

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

    // Sử dụng delegate để thông báo hoàn thành nhiệm vụ
    public Action<Quest> OnCompletedCallback { get; set; }
    public Action<Quest> OnQuestProgressUpdated; 
    
    // Khởi tạo nhiệm vụ
    public virtual void StartQuest()
    {
        state = QuestState.InProgress;
        SubscribeToEvents();
        Debug.Log($"Nhiệm vụ '{questName}' đã bắt đầu!");
    }

    // Hoàn thành nhiệm vụ
    protected void CompleteQuest()
    {
        if (state != QuestState.InProgress) return;
        state = QuestState.Completed;
        UnsubscribeFromEvents();
        OnCompletedCallback?.Invoke(this);
        Debug.Log($"Nhiệm vụ '{questName}' đã hoàn thành!");
    }

    // Nhiệm vụ thất bại
    protected void FailQuest()
    {
        if (state != QuestState.InProgress) return;
        state = QuestState.Failed;
        UnsubscribeFromEvents();
        // Cần thêm một delegate riêng cho sự kiện thất bại nếu bạn muốn
        Debug.Log($"Nhiệm vụ '{questName}' đã thất bại!");
    }

    // Các phương thức trừu tượng để đăng ký và hủy đăng ký sự kiện
    public abstract void SubscribeToEvents();
    public abstract void UnsubscribeFromEvents();
    public abstract void OnUpdate();
}