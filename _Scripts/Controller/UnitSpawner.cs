// File: UnitSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private Transform heroContainer;
    [SerializeField] private Transform enemyContainer;

    // Phương thức công khai được GameManager gọi cùng với LevelData
    public void SpawnAllUnits(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData rỗng. Không thể sinh đơn vị.");
            return;
        }

        SpawnHeroes(levelData.heroSpawnPositions);
        SpawnEnemies(levelData.enemySpawnPositions);
    }

    private void SpawnHeroes(List<Vector2Int> spawnPositions)
    {
        List<UnitData> playerUnitDataList = unitManager.GetPlayerUnitData();
        GameObject heroBasePrefab = unitManager.GetHeroBasePrefab();

        if (spawnPositions.Count == 0 || playerUnitDataList.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy vị trí sinh hero hoặc dữ liệu hero. Bỏ qua việc sinh hero.");
            return;
        }

        for (int i = 0; i < spawnPositions.Count && i < playerUnitDataList.Count; i++)
        {
            Vector2Int spawnPos = spawnPositions[i];
            Tile spawnTile = GridManager.Instance.GetTileAtPosition(spawnPos);
            
            if (spawnTile != null && !spawnTile.IsObstacle)
            {
                UnitData unitData = playerUnitDataList[i];
                GameObject unitObject = Instantiate(heroBasePrefab, Vector3.zero, Quaternion.identity, heroContainer);
                HeroUnit hero = unitObject.GetComponent<HeroUnit>();
                
                if (hero != null)
                {
                    hero.Setup(unitData);
                    // Đặt đơn vị lên ô gạch
                    spawnTile.PlaceUnit(hero);
                    Debug.Log($"Đã sinh Hero: {hero.name} tại {spawnPos}");
                }
            }
            else
            {
                Debug.LogWarning($"Không thể sinh hero tại {spawnPos}. Ô gạch không hợp lệ hoặc là chướng ngại vật.");
            }
        }
        
        // Di chuyển camera đến vị trí của hero đầu tiên nếu có
        if (spawnPositions.Count > 0)
        {
            FocusCameraOnUnit(spawnPositions[0]);
        }
    }

    private void SpawnEnemies(List<Vector2Int> spawnPositions)
    {
        
        List<UnitData> enemyUnitDataList = unitManager.GetEnemyUnitData();
        GameObject enemyBasePrefab = unitManager.GetEnemyBasePrefab();
        
        if (enemyUnitDataList.Count == 0 || spawnPositions.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy dữ liệu hoặc vị trí sinh enemy. Bỏ qua việc sinh enemy.");
            return;
        }

        // Trộn ngẫu nhiên các vị trí sinh để enemy xuất hiện ngẫu nhiên
        List<Vector2Int> shuffledSpawnPos = spawnPositions.OrderBy(x => Random.value).ToList();
        
        int enemyIndex = 0;
        int maxEnemies = GridManager.Instance.enemyUnitsToSpawn;
        Debug.Log($"Số enemy tối đa cần sinh: {maxEnemies}");

        foreach (Vector2Int cell in shuffledSpawnPos)
        {
            if (enemyIndex >= maxEnemies) break;

            Tile tile = GridManager.Instance.GetTileAtPosition(cell);
            if (tile == null || tile.IsObstacle || tile.tileData == null) continue;

            int maxPerTile = tile.tileData.maxEnemyPerTile;
            int availableSpace = tile.MaxUnitsPerTile - tile.occupyingUnits.Count;
            if (maxPerTile <= 0 || availableSpace <= 0) continue;

            int groupSize = Mathf.Min(maxPerTile, availableSpace);

            for (int i = 0; i < groupSize && enemyIndex < maxEnemies; i++)
            {
                UnitData unitData = enemyUnitDataList[enemyIndex % enemyUnitDataList.Count];
                GameObject unitObject = Instantiate(enemyBasePrefab, Vector3.zero, Quaternion.identity, enemyContainer);
                EnemyUnit enemy = unitObject.GetComponent<EnemyUnit>();

                if (enemy != null)
                {
                    enemy.Setup(unitData);
                    // Đặt đơn vị lên ô gạch
                    tile.PlaceUnit(enemy);
                    Debug.Log($"Đã sinh Enemy: {enemy.name} tại {cell}");
                }
                enemyIndex++;
            }
        }
    }

    private void FocusCameraOnUnit(Vector2Int unitCell)
    {
        Vector3 unitWorldPos = GridManager.Instance.GetWorldPosition(unitCell);
        Camera.main.transform.position = new Vector3(unitWorldPos.x, unitWorldPos.y, -10f);
    }
}