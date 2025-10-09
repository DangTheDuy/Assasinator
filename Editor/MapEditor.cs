// File: MapEditor.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MapEditor : EditorWindow
{
    private Dictionary<Vector2Int, TileData> tileMap = new Dictionary<Vector2Int, TileData>();
    private string[] tileTypes = { "grass", "forest", "mountain", "house", "water" };
    private int selectedTileIndex = 0;
    private int maxEnemyPerTile = 0;
    private int gridSize = 10;
    private bool markEnemySpawnZone = false;
    private int enemyUnitsToSpawn = 0;
    private int maxEnemySpawnTiles = 3;

    // Thêm LevelData để chỉnh sửa
    private LevelData targetLevelData;

    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditor>("Map Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("🗺️ Map Editor", EditorStyles.boldLabel);

        // Trường để chọn LevelData
        targetLevelData = (LevelData)EditorGUILayout.ObjectField("Level Data", targetLevelData, typeof(LevelData), false);

        if (targetLevelData == null)
        {
            EditorGUILayout.HelpBox("Hãy chọn một Level Data để chỉnh sửa.", MessageType.Info);
            return;
        }

        // Các nút Save/Load mới
        GUILayout.Space(10);
        if (GUILayout.Button("📥 Load from LevelData")) LoadFromLevelData();
        if (GUILayout.Button("📤 Save to LevelData")) SaveToLevelData();

        GUILayout.Space(10);
        selectedTileIndex = EditorGUILayout.Popup("Tile Type", selectedTileIndex, tileTypes);
        markEnemySpawnZone = EditorGUILayout.Toggle("Mark Enemy Spawn Zone", markEnemySpawnZone);
        enemyUnitsToSpawn = EditorGUILayout.IntField("Total Enemies", enemyUnitsToSpawn);
        maxEnemyPerTile = EditorGUILayout.IntField("Max Enemy/Tile", maxEnemyPerTile);    
        maxEnemySpawnTiles = EditorGUILayout.IntField("Max Tiles", maxEnemySpawnTiles);
        gridSize = EditorGUILayout.IntField("Grid Size", gridSize);

        GUILayout.Space(10);
        if (GUILayout.Button("🧹 Clear Map")) tileMap.Clear();

        GUILayout.Space(10);
        DrawGrid();
    }

    private void DrawGrid()
    {
        for (int y = gridSize - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                string label = "-";

                if (tileMap.ContainsKey(pos))
                {
                    var tile = tileMap[pos];
                    label = tile.isEnemySpawnZone
                        ? $"E{tile.maxEnemyPerTile}"
                        : tile.type.Substring(0, 1).ToUpper();
                }

                if (GUILayout.Button(label, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    tileMap[pos] = new TileData
                    {
                        x = pos.x,
                        y = pos.y,
                        type = tileTypes[selectedTileIndex],
                        isEnemySpawnZone = markEnemySpawnZone,
                        maxEnemyPerTile = maxEnemyPerTile
                    };
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    private void SaveToLevelData()
    {
        // Gán dữ liệu map từ Editor vào đối tượng LevelData
        if (targetLevelData.mapData == null)
            targetLevelData.mapData = new MapData();

        targetLevelData.mapData.tiles = new List<TileData>(tileMap.Values);
        targetLevelData.mapData.enemyUnitsToSpawn = enemyUnitsToSpawn;
        targetLevelData.mapData.maxEnemySpawnTiles = maxEnemySpawnTiles;
        targetLevelData.mapData.maxEnemyPerTile = maxEnemyPerTile;

        // Đánh dấu đối tượng LevelData là đã bị thay đổi để Unity lưu lại
        EditorUtility.SetDirty(targetLevelData);
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Map saved to {targetLevelData.name}.");
    }

    private void LoadFromLevelData()
    {
        // Tải dữ liệu map từ LevelData vào Editor
        if (targetLevelData.mapData == null || targetLevelData.mapData.tiles == null)
        {
            Debug.LogWarning($"⚠️ LevelData '{targetLevelData.name}' không có dữ liệu map.");
            tileMap.Clear();
            // Cần reset các giá trị khác để tránh sai lệch
            enemyUnitsToSpawn = 0;
            maxEnemySpawnTiles = 0;
            return;
        }

        tileMap.Clear();
        foreach (TileData data in targetLevelData.mapData.tiles)
        {
            Vector2Int pos = new Vector2Int(data.x, data.y);
            tileMap[pos] = data;
        }

        // Cập nhật các trường Editor với giá trị từ LevelData
        enemyUnitsToSpawn = targetLevelData.mapData.enemyUnitsToSpawn; // Dòng này đã được sửa
        maxEnemySpawnTiles = targetLevelData.mapData.maxEnemySpawnTiles; // Dòng này đã được sửa
        maxEnemyPerTile = targetLevelData.mapData.maxEnemyPerTile ;

        Debug.Log($"✅ Map loaded from {targetLevelData.name}.");
    }
}