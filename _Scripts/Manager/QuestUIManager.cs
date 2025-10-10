// File: QuestUIManager.cs
using System.Collections.Generic;
using UnityEngine;

public class QuestUIManager : Singleton<QuestUIManager>
{
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private Transform questListContainer;

    private readonly Dictionary<Quest, QuestUIItem> activeUIItems = new();

    private void OnEnable()
    {
        // Đăng ký sự kiện từ QuestSystem
        QuestSystem.OnQuestLoaded += AddQuestUI;
        QuestSystem.OnQuestCompleted += RemoveQuestUI;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh rò rỉ bộ nhớ
        QuestSystem.OnQuestLoaded -= AddQuestUI;
        QuestSystem.OnQuestCompleted -= RemoveQuestUI;
    }

    private void AddQuestUI(Quest quest)
    {
        if (activeUIItems.ContainsKey(quest)) return;

        GameObject uiItemObj = Instantiate(questItemPrefab, questListContainer);
        QuestUIItem uiItem = uiItemObj.GetComponent<QuestUIItem>();
        if (uiItem != null)
        {
            uiItem.Setup(quest);
            activeUIItems.Add(quest, uiItem);
        }
    }

    private void RemoveQuestUI(Quest quest)
    {
        if (activeUIItems.TryGetValue(quest, out QuestUIItem uiItem))
        {
            // Chúng ta sẽ giữ nhiệm vụ đã hoàn thành trên màn hình nhưng đánh dấu là đã hoàn thành
            uiItem.UpdateUI();
            
            // Bạn cũng có thể chọn xóa nó khỏi danh sách
            // activeUIItems.Remove(quest);
            // Destroy(uiItem.gameObject, 1f); // Xóa sau một khoảng trễ ngắn
        }
    }
}