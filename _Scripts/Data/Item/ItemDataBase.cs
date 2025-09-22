using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public static ItemDatabase Instance { get; private set; }

    [Header("Danh sách toàn bộ item có thể rơi")]
    public List<ItemData> allItems = new List<ItemData>();

    private void OnEnable()
    {
        Instance = this;
    }

    public ItemData GetRandomItem()
    {
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogWarning("⚠️ ItemDatabase trống, không có ItemData nào!");
            return null;
        }
        int idx = Random.Range(0, allItems.Count);
        return allItems[idx];
    }

    public ItemData GetItemByName(string itemName)
    {
        return allItems.Find(i => i.itemName == itemName);
    }
}
