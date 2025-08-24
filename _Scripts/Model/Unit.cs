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
     private static Unit selectedUnit;
     

    public Unit(UnitData unitData)
    {
        data = unitData;
    }

    public void SetPosition(Vector2Int pos)
    {
        Tile oldTile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (oldTile != null)
        {
            oldTile.SetUnoccupied(this);
        }

        currentPosition = pos;
    }

    private void OnMouseDown()
    {
        if (selectedUnit == this)
        {
            // Click lại chính nó -> bỏ chọn
            Debug.Log("DeSelect");
            DeSelect();
        }
        else
        {
            // Chọn unit mới
            if (selectedUnit != null)
            {
                // (tùy bạn, có thể bỏ highlight unit cũ tại đây)
            }

            selectedUnit = this;
            Debug.Log("Select " + name);
        }
    }
    public void DeSelect()
    {
        selectedUnit = null;
    }

    public static Unit GetSelectedUnit() => selectedUnit;


    public void MoveTo(Vector3 worldPos, Vector2Int gridPos)
    {
        // Giải phóng slot cũ
        Tile oldTile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (oldTile != null)
            oldTile.SetUnoccupied(this);

        // Gán slot mới ở tile đích
        Tile newTile = GridManager.Instance.GetTileAtPosition(gridPos);
        if (newTile != null)
        {
            newTile.SetOccupied(this); // slot gán ở đây duy nhất
            Vector3 offset = newTile.GetLocalOffsetForUnit(this);
            Vector3 basePos = GridManager.Instance.GetWorldPosition(gridPos);
            transform.position = new Vector3(basePos.x + offset.x, basePos.y + offset.y, -0.1f);
        }

        currentPosition = gridPos;
        DeSelect();
    }

}