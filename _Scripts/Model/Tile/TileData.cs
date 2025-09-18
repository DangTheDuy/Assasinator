

[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public string type;
    public bool isEnemySpawnZone;
    public int maxEnemyPerTile;
}

[System.Serializable]
public class TileDataList
{
    public TileData[] tiles;
    public int enemyUnitsToSpawn;
    public int maxEnemySpawnTiles;
}

