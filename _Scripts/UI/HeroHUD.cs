using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HeroHUD : MonoBehaviour
{
    [Header("UI References")]
    public Button heroIcon;
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI apText;
    public RectTransform itemContainer; // chính là "tấm thảm" sẽ scale theo X

    [Header("Roll Settings")]
    public float rollDuration = 0.35f;
    public Ease rollEaseOpen = Ease.OutBack;
    public Ease rollEaseClose = Ease.InCubic;
    public bool expandToRight = true; // mở ra về phải hay trái
    public float minScaleX = 0.001f; // tránh scale 0 tuyệt đối (1e-3 tốt hơn)

    private HeroUnit hero;
    private bool itemsVisible = false;

    // trạng thái lưu
    private Vector3 shownScale;
    private Vector3 hiddenScale;

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
    }

    private void InitItemContainer()
    {
        if (itemContainer == null) return;

        // lưu scale gốc (gọi là "mở ra" = 1)
        shownScale = itemContainer.localScale;

        // hiddenScale: chỉ thay đổi scale.x
        hiddenScale = new Vector3(minScaleX, shownScale.y, shownScale.z);

        // Đặt pivot để scale bắt đầu từ sát avatar
        // Nếu muốn mở sang phải thì pivot.x = 0 (trục trái cố định), ngược lại pivot.x = 1
        itemContainer.pivot = new Vector2(expandToRight ? 0f : 1f, itemContainer.pivot.y);

        // Đảm bảo vị trí itemContainer đặt ở vị trí "mở" mong muốn trong prefab.
        // Khi ẩn: set scale.x ~ 0 và deactivate (deactivate sẽ tắt, khi mở sẽ bật lại)
        itemContainer.localScale = hiddenScale;
        itemContainer.gameObject.SetActive(false);
    }

    private void OnHeroIconClick()
    {
        ToggleItems();
    }

    public void UpdateHP(int current, int max)
    {
        if (hpText != null) hpText.text = $"HP: {current}/{max}";
    }

    public void UpdateAP(int current, int max)
    {
        if (apText != null) apText.text = $"AP: {current}/{max}";
    }

    private void ToggleItems()
    {
        if (itemContainer == null) return;

        // hủy tween cũ nếu có
        itemContainer.DOKill();

        itemsVisible = !itemsVisible;

        if (itemsVisible)
        {
            // bật object trước khi tween (nếu đang inactive)
            itemContainer.gameObject.SetActive(true);

            // đảm bảo bắt đầu từ hiddenScale.x
            Vector3 start = hiddenScale;
            itemContainer.localScale = start;

            // tween scale → shownScale
            itemContainer.DOScale(shownScale, rollDuration)
                         .SetEase(rollEaseOpen);
        }
        else
        {
            // tween về hiddenScale rồi ẩn object
            itemContainer.DOScale(hiddenScale, rollDuration)
                         .SetEase(rollEaseClose)
                         .OnComplete(() => itemContainer.gameObject.SetActive(false));
        }
    }
}
