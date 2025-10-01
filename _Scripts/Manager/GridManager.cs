using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Tile Prefabs")]
    [SerializeField] private GameObject grassTilePrefab;
    [SerializeField] private GameObject forestTilePrefab;
    [SerializeField] private GameObject mountainTilePrefab;
    [SerializeField] private GameObject houseTilePrefab;
    [SerializeField] private GameObject obstacleTilePrefab;
    [SerializeField] private GameObject linePrefab;

    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 4f;
    public float TileSize => tileSize;

    public Vector2Int MapMin { get; private set; }
    public Vector2Int MapMax { get; private set; }

    public Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

    public int enemyUnitsToSpawn { get; private set; }
    public int maxEnemySpawnTiles { get; private set; }

    private void Awake()
    {
        Instance = this;
        LoadMapFromJson("map");
    }

    public void LoadMapFromJson(string fileName)
    {
        tiles.Clear();

        TextAsset jsonFile = Resources.Load<TextAsset>($"MapData/{fileName}");
        if (jsonFile == null)
        {
            Debug.LogError($"❌ Không tìm thấy file: Resources/MapData/{fileName}.json");
            return;
        }

        TileDataList mapData = JsonUtility.FromJson<TileDataList>(jsonFile.text);
        enemyUnitsToSpawn = mapData.enemyUnitsToSpawn;
        maxEnemySpawnTiles = mapData.maxEnemySpawnTiles;

        foreach (TileData data in mapData.tiles)
        {
            CreateTile(data);
        }
        CalculateMapBounds();
    }

    private void CreateTile(TileData data)
    {
        Vector2Int gridPos = new Vector2Int(data.x, data.y);
        GameObject prefab = GetTilePrefabByType(data.type);
        GameObject tileObj = Instantiate(prefab, GetWorldPosition(gridPos), Quaternion.identity);
        Tile tile = tileObj.GetComponent<Tile>();

        if (tile != null)
        {
            tile.name = $"Tile {data.x}, {data.y}";
            tile.transform.localScale = Vector3.one * tileSize * 0.99f;
            tile.Init(data.x, data.y, data);
            tiles[gridPos] = tile;
        }

        DrawGridOutline(tileObj.transform.position);
    }

    private GameObject GetTilePrefabByType(string type)
    {
        return type switch
        {
            "grass" => grassTilePrefab,
            "forest" => forestTilePrefab,
            "mountain" => mountainTilePrefab,
            "house" => houseTilePrefab,
            "obstacle" => obstacleTilePrefab,
            _ => grassTilePrefab
        };
    }

    private void DrawGridOutline(Vector3 center)
    {
        float half = tileSize / 2f;
        Vector3[] corners = new Vector3[]
        {
            center + new Vector3(-half, half, 0),
            center + new Vector3(half, half, 0),
            center + new Vector3(half, -half, 0),
            center + new Vector3(-half, -half, 0),
            center + new Vector3(-half, half, 0)
        };

        GameObject lineObj = Instantiate(linePrefab, center, Quaternion.identity);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = corners.Length;
        lr.SetPositions(corners);
    }

    private void CalculateMapBounds()
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var pos in tiles.Keys)
        {
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }

        MapMin = new Vector2Int(minX, minY);
        MapMax = new Vector2Int(maxX, maxY);
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        tiles.TryGetValue(position, out Tile tile);
        return tile;
    }

    public bool IsCellAvailableForMovement(Vector2Int position)
    {
        Tile tile = GetTileAtPosition(position);
        return tile != null && !tile.IsObstacle && tile.occupyingUnits.Count < tile.MaxUnitsPerTile;
    }

    public void SetCellOccupied(Vector2Int position, Unit unit)
    {
        GetTileAtPosition(position)?.SetOccupied(unit);
    }

    public void SetCellFree(Vector2Int position)
    {
        GetTileAtPosition(position)?.SetUnoccupied(null);
    }

    public List<Vector2Int> GetAvailableCells()
    {
        List<Vector2Int> available = new List<Vector2Int>();
        foreach (var kv in tiles)
        {
            if (IsCellAvailableForMovement(kv.Key))
                available.Add(kv.Key);
        }
        return available;
    }

    public List<Vector2Int> GetEnemySpawnCells()
    {
        List<Vector2Int> spawnCells = new List<Vector2Int>();
        foreach (var kv in tiles)
        {
            Tile tile = kv.Value;
            if (tile.tileData != null && tile.tileData.isEnemySpawnZone && !tile.IsObstacle)
                spawnCells.Add(kv.Key);
        }
        return spawnCells;
    }

    public Vector2Int GetCellPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / tileSize);
        int y = Mathf.FloorToInt(worldPosition.y / tileSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        Vector3 pos = new Vector3(gridPosition.x * tileSize, gridPosition.y * tileSize, 0);
        Debug.Log($"[GridManager] WorldPos từ grid {gridPosition} = {pos}");
        return pos;
    }


    public int GetDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}
