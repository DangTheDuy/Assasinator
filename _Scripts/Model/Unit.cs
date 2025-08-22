using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Unit : MonoBehaviour
{
    public UnitData data { get; private set; }

    public Vector2Int currentPosition { get; set; }
    public Sprite Image => data.Image;
    private int currentHealth => data.maxHealth;
    private int currentAttack => data.attackPower;
    private int currentDefend => data.defensePower;

    public Unit(UnitData unitData)
    {
        data = unitData;
    }

    public void SetPosition(Vector2Int pos)
    {
        currentPosition = pos;
    }

    public void MoveTo(Vector3 worldPos)
    {

    }
}