
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
            UnitData unitData = playerUnitDataList[i];
            Vector3 basePosition = GridManager.Instance.GetWorldPosition(heroSpawnCell);
            Vector3 offset = heroTile.GetLocalOffsetForUnit(i);
            Vector3 spawnPosition = basePosition + offset;

            GameObject unitObject = Instantiate(heroBasePrefab, spawnPosition, Quaternion.identity);
            unitObject.transform.position = new Vector3(unitObject.transform.position.x,unitObject.transform.position.y,-1f );
            Unit unitScript = unitObject.GetComponent<Unit>();
            if (unitScript != null)
            {
                unitScript.SetPosition(heroSpawnCell);     
                heroTile.SetOccupied(unitScript);           
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
                UnitData unitData = enemyUnitDataList[enemyIndex % enemyUnitDataList.Count];
                Vector3 basePosition = GridManager.Instance.GetWorldPosition(groupCell);
                Vector3 offset = groupTile.GetLocalOffsetForUnit(i); // ✅ sửa chỗ này
                Vector3 spawnPosition = basePosition + offset;

                GameObject unitObject = Instantiate(enemyBasePrefab, spawnPosition, Quaternion.identity);
                unitObject.transform.position = new Vector3(unitObject.transform.position.x,unitObject.transform.position.y,-1f );
                Unit unitScript = unitObject.GetComponent<Unit>();
                if (unitScript != null)
                {
                    unitScript.SetPosition(groupCell);
                    groupTile.SetOccupied(unitScript);
                }
                enemyIndex++;
            }
        }
    }
}
