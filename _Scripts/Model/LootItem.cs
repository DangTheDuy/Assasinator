using UnityEngine;

public class LootItem : MonoBehaviour
{
    public ItemData itemData; 
    public int quantity = 1;
    private Tile parentTile;

    public void Init(ItemData data, int qty, Tile tile)
    {
        parentTile = tile;

        // Nếu Init không truyền data thì random luôn
        if (data == null)
        {
            itemData = ItemDatabase.Instance.GetRandomItem();
            quantity = 1;
        }
        else
        {
            itemData = data;
            quantity = qty;
        }
    }

    private void Start()
    {
        // Nếu vì lý do nào đó Init không gọi, vẫn fallback
        if (itemData == null)
        {
            itemData = ItemDatabase.Instance.GetRandomItem();
            quantity = 1;
        }
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
        if (hero == null || itemData == null) return;

        hero.AddItem(itemData, quantity);
        Debug.Log($"{hero.name} nhặt được {quantity} x {itemData.itemName}");

        if (parentTile != null)
            parentTile.RemoveLoot(this);

        Destroy(gameObject);
    }
}
