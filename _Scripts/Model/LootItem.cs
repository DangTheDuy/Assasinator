using UnityEngine;

public class LootItem : MonoBehaviour
{
    public ItemData itemData; 
    public int quantity = 1;
    private Tile parentTile;

    public void Init(ItemData data, int qty, Tile tile)
    {
        itemData = data;
        quantity = qty;
        parentTile = tile;
    }

    private void OnMouseDown()
    {
        HeroUnit hero = HeroUnit.SelectedHero;
        if (hero == null)
        {
            Debug.Log("[LootItem] Không có Hero nào được chọn!");
            return;
        }
        
        if (hero.currentPosition != parentTile.gridPosition)
        {
            Debug.Log("[LootItem] Hero không đứng cùng tile với loot -> không thể nhặt.");
            return;
        }

        Collect(hero);
    }

    public void Collect(HeroUnit hero)
    {
        if (hero == null) return;

        if (itemData == null)
        {
            itemData = ItemDatabase.Instance.GetRandomItem();
            quantity = 1;
        }

        if (itemData == null)
        {
            Debug.LogWarning("[LootItem] Không tìm thấy ItemData!");
            return;
        }

        hero.AddItem(itemData, quantity);
        Debug.Log($"{hero.name} nhặt được {quantity} x {itemData.itemName}");

        if (parentTile != null)
            parentTile.RemoveLoot(this);

        Destroy(gameObject);
    }
}
