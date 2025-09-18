using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MapEditor : EditorWindow
{
    private Dictionary<Vector2Int, TileData> tileMap = new Dictionary<Vector2Int, TileData>();
    private string[] tileTypes = { "grass", "forest", "mountain", "house", "obstacle" };
    private int selectedTileIndex = 0;
    private int gridSize = 10;
    private string exportFileName = "map";
    private bool markEnemySpawnZone = false;

    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditor>("Map Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Editor", EditorStyles.boldLabel);
        selectedTileIndex = EditorGUILayout.Popup("Tile Type", selectedTileIndex, tileTypes);
        markEnemySpawnZone = EditorGUILayout.Toggle("Mark Enemy Spawn Zone", markEnemySpawnZone);
        gridSize = EditorGUILayout.IntField("Grid Size", gridSize);
        exportFileName = EditorGUILayout.TextField("Export File Name", exportFileName);

        GUILayout.Space(10);
        if (GUILayout.Button("Clear Map")) tileMap.Clear();
        if (GUILayout.Button("Export to JSON")) ExportToJson();
        if (GUILayout.Button("Load from JSON")) LoadFromJson();

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
                    label = tile.isEnemySpawnZone ? "E" : tile.type.Substring(0, 1).ToUpper();
                }

                if (GUILayout.Button(label, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    tileMap[pos] = new TileData
                    {
                        x = pos.x,
                        y = pos.y,
                        type = tileTypes[selectedTileIndex],
                        isEnemySpawnZone = markEnemySpawnZone
                    };
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    private void ExportToJson()
    {
        List<TileData> tiles = new List<TileData>();
        foreach (var kv in tileMap)
        {
            tiles.Add(kv.Value);
        }

        string json = JsonUtility.ToJson(new TileDataList { tiles = tiles.ToArray() }, true);
        string path = Application.dataPath + $"/Resources/MapData/{exportFileName}.json";
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        Debug.Log($"✅ Map exported to {path}");
    }

    private void LoadFromJson()
    {
        string path = Application.dataPath + $"/Resources/MapData/{exportFileName}.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning($"⚠️ Không tìm thấy file: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        TileDataList tileDataList = JsonUtility.FromJson<TileDataList>(json);

        tileMap.Clear();
        foreach (TileData data in tileDataList.tiles)
        {
            Vector2Int pos = new Vector2Int(data.x, data.y);
            tileMap[pos] = data;
        }

        Debug.Log($"✅ Map loaded from {path}");
    }

}
