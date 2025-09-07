using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public Image icon;
    public static List<Unit> AllUnits = new List<Unit>();


    public virtual void Setup(UnitData unitData)
    {
        data = unitData;
        currentHealth = data.maxHealth;
        currentAttack = data.attackPower;
        currentDefend = data.defensePower;

        if (icon != null && data.Image != null)
        {
            icon.sprite = data.Image;
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

        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsTargeting)
        {
            TargetingSystem.Instance.ExitTargetMode();
            Debug.Log($"[Unit] Hero bỏ chọn -> ExitTargetMode()");
        }
    }

    private void OnDestroy()
    {
        if (AllUnits.Contains(this))
            AllUnits.Remove(this);

        Tile tile = GridManager.Instance.GetTileAtPosition(currentPosition);
        if (tile != null)
            tile.SetUnoccupied(this);

        if (SelectedEnemy == this) SelectedEnemy = null;
        if (SelectedHero == this) SelectedHero = null;

        SkillBarUI skillBar = FindObjectOfType<SkillBarUI>();
        if (skillBar != null && skillBar.gameObject.activeSelf)
        {
            WorldSpaceUIFollow follow = skillBar.GetComponent<WorldSpaceUIFollow>();
            if (follow != null && follow.target == this.transform)
            {
                skillBar.Hide();
            }
        }
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
        if (this is HeroUnit hero && SelectedHero == hero)
        {
            // Tắt highlight cũ
            foreach (var kv in GridManager.Instance.tiles)
            {
                kv.Value.Highlight(false);
            }

            // Tính lại highlight cho các ô mới trong range
            foreach (var kv in GridManager.Instance.tiles)
            {
                int distance = GridManager.Instance.GetDistance(currentPosition, kv.Key);
                if (distance <= data.moveRange && GridManager.Instance.IsCellAvailableForMovement(kv.Key))
                {
                    kv.Value.Highlight(true);
                }
            }
        }
    }
}