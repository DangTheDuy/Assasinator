using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
    public List<TileData> tiles;
    public int enemyUnitsToSpawn;
    public int maxEnemySpawnTiles;
    public int maxEnemyPerTile;
}