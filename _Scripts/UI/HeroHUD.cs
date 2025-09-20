using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HeroHUD : MonoBehaviour
{
    [Header("UI References")]
    public Image heroIcon;
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI apText;
    public Transform itemContainer;
 //   public GameObject itemSlotPrefab; // Prefab của 1 ô item (Image)

    private HeroUnit hero;

    public void Setup(HeroUnit heroUnit)
    {
        hero = heroUnit;

        if (heroIcon != null && hero.data.Image != null)
            heroIcon.sprite = hero.data.Image;

        heroNameText.text = hero.data.unitName;

        UpdateHP(hero.CurrentHP, hero.data.maxHealth);
        UpdateAP(hero.currentAP, hero.data.maxAP);

     //   UpdateItems(new List<Sprite>()); // ban đầu rỗng
    }

    public void UpdateHP(int current, int max)
    {
        hpText.text = $"HP: {current}/{max}";
    }

    public void UpdateAP(int current, int max)
    {
        apText.text = $"AP: {current}/{max}";
    }

 /*   public void UpdateItems(List<Sprite> itemIcons)
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (var icon in itemIcons)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
            slot.GetComponent<Image>().sprite = icon;
        }
    }*/
}
