// File: QuestUIItem.cs
using TMPro;
using UnityEngine;

public class QuestUIItem : MonoBehaviour
{
    private TextMeshProUGUI questText;
    private Quest assignedQuest;

    private void Awake()
    {
        questText = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(Quest quest)
    {
        assignedQuest = quest;
        UpdateUI();
        // ✨ Đăng ký sự kiện cập nhật tiến độ
        assignedQuest.OnQuestProgressUpdated += OnProgressUpdated; 
    }

    private void OnDestroy()
    {
        if (assignedQuest != null)
        {
            // ✨ Hủy đăng ký sự kiện cập nhật tiến độ
            assignedQuest.OnQuestProgressUpdated -= OnProgressUpdated; 
        }
    }
    
    // Phương thức xử lý sự kiện cập nhật tiến độ
    private void OnProgressUpdated(Quest quest)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (assignedQuest == null)
        {
            questText.text = "Không có nhiệm vụ";
            return;
        }

        string progressInfo = "";
        if (assignedQuest is EliminateEnemiesQuest elimQuest)
        {
            progressInfo = $" ({elimQuest.CurrentKills}/{elimQuest.TargetCount})";
        }
        else if (assignedQuest is MoveToLocationQuest moveQuest)
        {
            if (assignedQuest.state == QuestState.Completed)
            {
                progressInfo = " V";
            }
        }

        questText.text = $"{assignedQuest.questName}{progressInfo}";

        if (assignedQuest.state == QuestState.Completed)
        {
            questText.color = Color.gray;
        }
    }
}