using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class HeroHUD : MonoBehaviour
{
    [Header("UI References")]
    public Button heroIcon;
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI apText;
    public RectTransform itemContainer;
    public GameObject itemSlotPrefab; // prefab: Button (Image + Text số lượng)

    [Header("Roll Settings")]
    public float rollDuration = 0.35f;
    public Ease rollEaseOpen = Ease.OutBack;
    public Ease rollEaseClose = Ease.InCubic;
    public bool expandToRight = true;
    public float minScaleX = 0.001f;

    private HeroUnit hero;
    private bool itemsVisible = false;
    private Vector3 shownScale;
    private Vector3 hiddenScale;

    // =================================================== SETUP ===================================================
    public void Setup(HeroUnit heroUnit)
    {
        hero = heroUnit;

        if (heroIcon != null && hero.data.Image != null)
            heroIcon.image.sprite = hero.data.Image;

        heroNameText.text = hero.data.unitName;
        UpdateHP(hero.CurrentHP, hero.data.maxHealth);
        UpdateAP(hero.currentAP, hero.data.maxAP);

        InitItemContainer();

        heroIcon.onClick.RemoveAllListeners();
        heroIcon.onClick.AddListener(OnHeroIconClick);

        // load items ban đầu
        UpdateItems(hero.inventory);
    }

    private void InitItemContainer()
    {
        if (itemContainer == null) return;

        shownScale = itemContainer.localScale;
        hiddenScale = new Vector3(minScaleX, shownScale.y, shownScale.z);

        itemContainer.pivot = new Vector2(expandToRight ? 0f : 1f, itemContainer.pivot.y);

        itemContainer.localScale = hiddenScale;
        itemContainer.gameObject.SetActive(false);
    }

    // =================================================== UPDATE STATS ===================================================
    public void UpdateHP(int current, int max)
    {
        if (hpText != null) hpText.text = $"HP: {current}/{max}";
    }

    public void UpdateAP(int current, int max)
    {
        if (apText != null) apText.text = $"AP: {current}/{max}";
    }

    // =================================================== ITEMS ===================================================
    public void UpdateItems(List<ItemStack> items)
    {
        if (itemContainer == null || itemSlotPrefab == null) return;

        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (var stack in items)
        {
            if (stack == null || stack.itemData == null) continue;

            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);

            // icon
            Image img = slot.GetComponent<Image>();
            if (img != null && stack.itemData.icon != null)
                img.sprite = stack.itemData.icon;

            // số lượng
            TextMeshProUGUI qtyText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (qtyText != null)
                qtyText.text = stack.quantity.ToString();

            // button click
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (hero != null)
                        hero.UseItem(stack);
                });
            }
        }
    }

    // =================================================== TOGGLE UI ===================================================
    private void OnHeroIconClick()
    {
        ToggleItems();
    }

    private void ToggleItems()
    {
        if (itemContainer == null) return;

        itemContainer.DOKill();
        itemsVisible = !itemsVisible;

        if (itemsVisible)
        {
            itemContainer.gameObject.SetActive(true);
            itemContainer.localScale = hiddenScale;
            itemContainer.DOScale(shownScale, rollDuration).SetEase(rollEaseOpen);
        }
        else
        {
            itemContainer.DOScale(hiddenScale, rollDuration)
                         .SetEase(rollEaseClose)
                         .OnComplete(() => itemContainer.gameObject.SetActive(false));
        }
    }
}
