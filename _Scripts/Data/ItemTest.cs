using UnityEngine;

public class ItemTest : MonoBehaviour
{
    public ItemData healPotion;
    public ItemData apPotion;

    private HeroUnit hero;

    void OnEnable()
    {
        HeroUnit.OnHeroSpawned += HandleHeroSpawned;
    }

    void OnDisable()
    {
        HeroUnit.OnHeroSpawned -= HandleHeroSpawned;
    }

    private void HandleHeroSpawned(HeroUnit spawnedHero)
    {
        hero = spawnedHero;
        Debug.Log($"[ItemTest] Hero {hero.name} đã spawn xong");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[ItemTest] Nhấn H");
            if (hero == null)
            {
                Debug.LogWarning("[ItemTest] Hero chưa spawn!");
                return;
            }

            hero.AddItem(healPotion, 1);
            Debug.Log($"[ItemTest] Đã thêm 1 {healPotion.itemName}, tổng: {hero.inventory.Count} items");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("[ItemTest] Nhấn A");
            if (hero == null)
            {
                Debug.LogWarning("[ItemTest] Hero chưa spawn!");
                return;
            }

            hero.AddItem(apPotion, 1);
            Debug.Log($"[ItemTest] Đã thêm 1 {apPotion.itemName}, tổng: {hero.inventory.Count} items");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[ItemTest] Nhấn U");
            if (hero == null)
            {
                Debug.LogWarning("[ItemTest] Hero chưa spawn!");
                return;
            }

            if (hero.inventory.Count > 0)
            {
                var stack = hero.inventory[0];
                Debug.Log($"[ItemTest] Dùng item {stack.itemData.itemName}, còn {stack.quantity} trước khi dùng");
                hero.UseItem(stack);
            }
            else
            {
                Debug.Log("[ItemTest] Hero không có item để dùng!");
            }
        }
    }
}
