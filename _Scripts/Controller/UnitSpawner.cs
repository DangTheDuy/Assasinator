using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private Transform heroContainer;
    [SerializeField] private Transform enemyContainer;

    private int totalEnemiesSpawned = 0;

    public void Start()
    {
        SpawnAllUnitsCustom();
    }

    public void SpawnAllUnitsCustom()
    {
        List<UnitData> playerUnitDataList = unitManager.GetPlayerUnitData();
        List<UnitData> enemyUnitDataList = unitManager.GetEnemyUnitData();

        GameObject heroBasePrefab = unitManager.GetHeroBasePrefab();
        GameObject enemyBasePrefab = unitManager.GetEnemyBasePrefab();

        // SPAWN HERO
        Vector2Int heroSpawnCell = new Vector2Int(1, 1);
        Tile heroTile = GridManager.Instance.GetTileAtPosition(heroSpawnCell);

        for (int i = 0; i < Mathf.Min(playerUnitDataList.Count, 2); i++)
        {
            UnitData unitData = playerUnitDataList[i];
            GameObject unitObject = Instantiate(heroBasePrefab, Vector3.zero, Quaternion.identity, heroContainer);
            HeroUnit hero = unitObject.GetComponent<HeroUnit>();
            if (hero != null)
            {
                heroTile.PlaceUnit(hero);
                hero.Setup(unitData);
            }
        }

        // SPAWN ENEMY
        int enemyUnitsToSpawn = GridManager.Instance.enemyUnitsToSpawn;
        int maxSpawnTiles = GridManager.Instance.maxEnemySpawnTiles;

        List<Vector2Int> allSpawnCells = GridManager.Instance.GetEnemySpawnCells();
        allSpawnCells.Remove(heroSpawnCell);

        int usableTiles = Mathf.Min(maxSpawnTiles, allSpawnCells.Count);
        List<Vector2Int> selectedTiles = new List<Vector2Int>();

        while (selectedTiles.Count < usableTiles)
        {
            Vector2Int cell = allSpawnCells[Random.Range(0, allSpawnCells.Count)];
            allSpawnCells.Remove(cell);
            selectedTiles.Add(cell);
        }

        int enemyIndex = 0;
        totalEnemiesSpawned = 0;

        foreach (Vector2Int cell in selectedTiles)
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(cell);
            if (tile == null || tile.IsObstacle || tile.tileData == null) continue;

            int maxPerTile = tile.tileData.maxEnemyPerTile;
            int availableSpace = tile.MaxUnitsPerTile - tile.occupyingUnits.Count;
            if (maxPerTile <= 0 || availableSpace <= 0) continue;

            int groupSize = Mathf.Min(maxPerTile, availableSpace);

            for (int i = 0; i < groupSize && enemyIndex < enemyUnitsToSpawn; i++)
            {
                UnitData unitData = enemyUnitDataList[enemyIndex % enemyUnitDataList.Count];
                GameObject unitObject = Instantiate(enemyBasePrefab, Vector3.zero, Quaternion.identity, enemyContainer);
                EnemyUnit enemy = unitObject.GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    enemy.Setup(unitData);
                    tile.PlaceUnit(enemy);
                    tile.UpdateUnitVisibility();
                    totalEnemiesSpawned++;
                }
                enemyIndex++;
            }
        }
        FocusCameraOnHero(heroSpawnCell);
    }

    public void FocusCameraOnHero(Vector2Int heroCell)
    {
        Vector3 heroWorldPos = GridManager.Instance.GetWorldPosition(heroCell);
        Camera.main.transform.position = new Vector3(heroWorldPos.x, heroWorldPos.y, -10f);
    }

}
