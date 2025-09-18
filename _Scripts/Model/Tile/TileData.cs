using System;

[Serializable]
public class TileData
{
    public int x;
    public int y;
    public string type;
}

[Serializable]
public class TileDataList
{
    public TileData[] tiles;
}
