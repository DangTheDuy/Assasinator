
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Singleton<UnitManager>
{
    [SerializeField] private GameObject heroBasePrefab;
    [SerializeField] private GameObject enemyBasePrefab;

    private string playerDataPath = "Units/Player";
    private string enemyDataPath = "Units/Enemy";

    private readonly List<UnitData> playerUnitDataList = new();
    private readonly List<UnitData> enemyUnitDataList = new();

    protected override void Awake()
    {
        base.Awake();
        LoadUnitData();
    }

    private void LoadUnitData()
    {
        UnitData[] playerData = Resources.LoadAll<UnitData>(playerDataPath);
        playerUnitDataList.AddRange(playerData);

        UnitData[] enemyData = Resources.LoadAll<UnitData>(enemyDataPath);
        enemyUnitDataList.AddRange(enemyData);
    }

    public List<UnitData> GetPlayerUnitData() => playerUnitDataList;
    public List<UnitData> GetEnemyUnitData() => enemyUnitDataList;

    public GameObject GetHeroBasePrefab() => heroBasePrefab;
    public GameObject GetEnemyBasePrefab() => enemyBasePrefab;
}
