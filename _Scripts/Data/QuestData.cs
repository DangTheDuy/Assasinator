// File: QuestData.cs
using UnityEngine;

// Enum cho các loại nhiệm vụ
public enum QuestType
{
    EliminateEnemies,
    MoveToLocation,
    // Thêm các loại nhiệm vụ khác ở đây
}

[System.Serializable]
public class QuestData
{
    public QuestType questType;

    [Header("Eliminate Enemies")]
    public int targetEnemyCount = 1;
    public string targetEnemyName; // Optional: Tên kẻ địch cụ thể

    [Header("Move To Location")]
    public Vector2Int targetPosition;
}