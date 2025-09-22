using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public ItemData itemData;
    public int quantity;

    public ItemStack(ItemData data, int qty = 1)
    {
        itemData = data;
        quantity = qty;
    }

    public void Add(int amount) => quantity += amount;
    public bool Consume()
    {
        if (quantity > 0)
        {
            quantity--;
            return true;
        }
        return false;
    }
}

