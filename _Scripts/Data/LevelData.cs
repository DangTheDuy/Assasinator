using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    
    [Header("Map Data")]
    public MapData mapData;

    [Header("Player")]
    public List<Vector2Int> heroSpawnPositions = new List<Vector2Int>();

    [Header("Enemies")]
    public List<Vector2Int> enemySpawnPositions = new List<Vector2Int>();
    
    [Header("Quests")]
    public List<QuestData> quests;
}