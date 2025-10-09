// File: GameManager.cs
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Level Configuration")]
    public LevelData currentLevelData;

    [Header("Dependencies")]
    [SerializeField] private UnitSpawner unitSpawner;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("❌ LevelData chưa được gán trong GameManager!");
            return;
        }

        // Tải bản đồ từ LevelData
        GridManager.Instance.LoadMapFromData(currentLevelData.mapData);

        // Sinh hero và enemy bằng UnitSpawner
        unitSpawner.SpawnAllUnits(currentLevelData);

        // Cập nhật danh sách kẻ thù trong EnemySystem sau khi sinh
        EnemySystem.Instance.RefreshEnemies();
        
        // Khởi tạo các nhiệm vụ
        QuestSystem.Instance.InitializeQuests(currentLevelData);

        // Bắt đầu lượt chơi
        TurnManager.Instance.StartGame();
    }
}