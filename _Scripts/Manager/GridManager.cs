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

    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 4f;

    public Dictionary<Vector2Int, Tile> tiles;

    private void Awake()
    {
        Instance = this;
        GenerateGridFromJson();
    }

    // ======================= JSON MAP LOADING ===========================
    public void GenerateGridFromJson()
    {
        tiles = new Dictionary<Vector2Int, Tile>();

        TextAsset jsonFile = Resources.Load<TextAsset>("MapData/map");
        if (jsonFile == null)
        {
            Debug.LogError("Không tìm thấy file map.json trong Resources/MapData/");
            return;
        }

        TileDataList tileList = JsonUtility.FromJson<TileDataList>(jsonFile.text);

        foreach (TileData data in tileList.tiles)
        {
            Vector2Int gridPos = new Vector2Int(data.x, data.y);
            GameObject prefab = GetTilePrefabByType(data.type);
            var tileObj = Instantiate(prefab, GetWorldPosition(gridPos), Quaternion.identity);
            Tile tile = tileObj.GetComponent<Tile>();

            if (tile != null)
            {
                tile.name = $"Tile {data.x}, {data.y}";
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);
                tile.Init(data.x, data.y);
                tiles[gridPos] = tile;
            }
        }
    }

    private GameObject GetTilePrefabByType(string type)
    {
        switch (type)
        {
            case "grass": return grassTilePrefab;
            case "forest": return forestTilePrefab;
            case "mountain": return mountainTilePrefab;
            case "house": return houseTilePrefab;
            case "obstacle": return obstacleTilePrefab;
            default: return grassTilePrefab;
        }
    }

    // ======================= TILE ACCESS ===========================
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
        Tile tile = GetTileAtPosition(position);
        tile?.SetOccupied(unit);
    }

    public void SetCellFree(Vector2Int position)
    {
        Tile tile = GetTileAtPosition(position);
        tile?.SetUnoccupied(unit: null); // hoặc truyền unit nếu cần
    }

    public List<Vector2Int> GetAvailableCells()
    {
        List<Vector2Int> availableCells = new List<Vector2Int>();
        foreach (var cell in tiles.Keys)
        {
            if (IsCellAvailableForMovement(cell))
                availableCells.Add(cell);
        }
        return availableCells;
    }

    // ======================= POSITION CONVERSION ===========================
    public Vector2Int GetCellPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / tileSize);
        int y = Mathf.FloorToInt(worldPosition.y / tileSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * tileSize, gridPosition.y * tileSize, 0);
    }

    public int GetDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}

