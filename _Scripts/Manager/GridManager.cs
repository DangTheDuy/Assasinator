
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private int width, height;
    [SerializeField] private Tile grassTile;
    public Dictionary<Vector2Int, Tile> tiles;

    private float tileSize = 4f;

    void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    void Start()
    {

    }

   public void GenerateGrid()
    {
        tiles = new Dictionary<Vector2Int, Tile>();
        float tileScale = 4f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                var spawnedTileObject = Instantiate(grassTile, new Vector3(x * tileScale, y * tileScale), quaternion.identity);
                Tile spawnedTile = spawnedTileObject.GetComponent<Tile>();

                if (spawnedTile != null)
                {
                    spawnedTile.name = $"Tile {x}, {y}";
                    spawnedTile.transform.localScale = new Vector3(tileScale, tileScale, 1);
                    spawnedTile.Init(x, y);

                    // 🔥 Thêm caro: đổi màu dựa theo x+y
                    SpriteRenderer sr = spawnedTile.GetComponent<SpriteRenderer>();
                    if ((x + y) % 2 == 0)
                    {
                        sr.color = new Color(0.8f, 0.8f, 0.8f); // màu sáng
                    }
                    else
                    {
                        sr.color = new Color(0.4f, 0.4f, 0.4f); // màu tối
                    }

                    tiles[gridPos] = spawnedTile;
                }
            }
        }
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        if (tiles.ContainsKey(position))
        {
            return tiles[position];
        }
        return null;
    }

     public bool IsCellAvailableForMovement(Vector2Int position)
    {
        Tile tile = GetTileAtPosition(position);
        return tile != null && !tile.IsObstacle && tile.occupyingUnits.Count < tile.MaxUnitsPerTile;
    }


    public void SetCellOccupied(Vector2Int position, Unit unit)
    {
        Tile tile = GetTileAtPosition(position);
        if (tile != null)
        {
            tile.SetOccupied(unit);
        }
    }

    public void SetCellFree(Vector2Int position)
    {
        Tile tile = GetTileAtPosition(position);
    }

    public List<Vector2Int> GetAvailableCells()
    {
        List<Vector2Int> availableCells = new List<Vector2Int>();
        foreach (var cell in tiles.Keys)
        {
            if (IsCellAvailableForMovement(cell))
            {
                availableCells.Add(cell);
            }
        }
        return availableCells;
    }

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


}
