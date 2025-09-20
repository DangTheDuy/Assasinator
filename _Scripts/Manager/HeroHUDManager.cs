using UnityEngine;
using System.Collections.Generic;

public class HeroHUDManager : MonoBehaviour
{
    public GameObject heroHudPrefab; 
    public Transform heroHudContainer; 
    private Dictionary<HeroUnit, HeroHUD> hudMap = new();

    public void CreateHUD(HeroUnit hero)
    {
        GameObject hudObj = Instantiate(heroHudPrefab, heroHudContainer);
        HeroHUD hud = hudObj.GetComponent<HeroHUD>();
        hud.Setup(hero);
        hudMap[hero] = hud;
    }

    // Các hàm update để Hero gọi khi thay đổi chỉ số
    public void UpdateHeroHP(HeroUnit hero, int current, int max)
    {
        if (hudMap.ContainsKey(hero))
            hudMap[hero].UpdateHP(current, max);
    }

    public void UpdateHeroAP(HeroUnit hero, int current, int max)
    {
        if (hudMap.ContainsKey(hero))
            hudMap[hero].UpdateAP(current, max);
    }

   /* public void UpdateHeroItems(HeroUnit hero, List<Sprite> items)
    {
        if (hudMap.ContainsKey(hero))
            hudMap[hero].UpdateItems(items);
    }*/
}
