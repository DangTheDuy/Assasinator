
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private int playerUnitsToSpawn ;
    [SerializeField] private int enemyUnitsToSpawn ;

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

        for (int i = 0; i < Mathf.Min(playerUnitsToSpawn, playerUnitDataList.Count); i++)
        {
            GameObject unitObject = Instantiate(heroBasePrefab, Vector3.zero, Quaternion.identity);

            Unit unitScript = unitObject.GetComponent<Unit>();
            if (unitScript != null)
            {
                heroTile.PlaceUnit(unitScript);         
            }
        }

        Vector3 heroWorldPos = GridManager.Instance.GetWorldPosition(heroSpawnCell);
        Camera.main.transform.position = new Vector3(heroWorldPos.x, heroWorldPos.y, -10f);

    // SPAWN ENEMY
        List<Vector2Int> availableCells = GridManager.Instance.GetAvailableCells();
        availableCells.Remove(heroSpawnCell);

        int enemyIndex = 0;
        while (enemyIndex < enemyUnitsToSpawn && availableCells.Count > 0)
        {
            Vector2Int groupCell = availableCells[0];
            availableCells.RemoveAt(0);

            Tile groupTile = GridManager.Instance.GetTileAtPosition(groupCell);
            if (groupTile == null || groupTile.IsObstacle) continue;

            int groupSize = Mathf.Min(Random.Range(3, 5), groupTile.MaxUnitsPerTile - groupTile.occupyingUnits.Count);

            for (int i = 0; i < groupSize && enemyIndex < enemyUnitDataList.Count; i++)
            {
                GameObject unitObject = Instantiate(enemyBasePrefab, Vector3.zero, Quaternion.identity);

                Unit unitScript = unitObject.GetComponent<Unit>();
                if (unitScript != null)
                {
                    groupTile.PlaceUnit(unitScript); 
                }
                enemyIndex++;
            }
        }
    }
}
