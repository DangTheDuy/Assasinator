[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public string type;
    public bool isEnemySpawnZone;
}

[System.Serializable]
public class TileDataList
{
    public TileData[] tiles;
}
