using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Sprite Image;
    public int moveRange;
    public int attackRange;
    public int maxHealth;
    public int attackPower;
    public int defensePower;
    public int teamID;
    public List<SkillData> skills;
    [Range(0, 100)] public int detectionChance = 10;
}
