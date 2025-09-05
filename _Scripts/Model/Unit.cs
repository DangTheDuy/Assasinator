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
    private int currentHealth ;
    private int currentAttack ;
    private int currentDefend ;
    public static HeroUnit SelectedHero;
    public static EnemyUnit SelectedEnemy;
    public static List<Unit> AllUnits = new List<Unit>();


    public virtual void Setup(UnitData unitData)
    {
        data = unitData;
        currentHealth = data.maxHealth;
        currentAttack = data.attackPower;
        currentDefend = data.defensePower;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.Image != null)
        {
            sr.sprite = data.Image;
        }
        name = data.unitName;

        if (!AllUnits.Contains(this))
            AllUnits.Add(this);
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

        if (this is EnemyUnit enemy)
        {
            SelectedEnemy = enemy; 
            Debug.Log($"Đã chọn target {enemy.name}");
            return; 
        }

        if (SelectedHero == this)
        {
            // Click lại chính nó -> bỏ chọn
            OnDeselect();
        }
        else
        {
            // Chọn unit mới
            if (SelectedHero != null)
            {
                // (tùy bạn, có thể bỏ highlight unit cũ tại đây)
            }
            OnSelect();

        }
    }

    public virtual void OnSelect()
    {

    }

    public virtual void OnDeselect()
    {
        SelectedHero = null;
    }

    private void OnDestroy()
    {
        AllUnits.Remove(this);

        if (GridManager.Instance != null)
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(currentPosition);
            if (tile != null)
                tile.SetUnoccupied(this);
        }

        if (SelectedEnemy == this) SelectedEnemy = null;
        if (SelectedHero == this) SelectedHero = null;
    }


     public static HeroUnit GetSelectedUnit() => SelectedHero;

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
        OnDeselect();
    }

}