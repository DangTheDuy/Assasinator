using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public SkillBarUI skillBarPrefab;   // kéo prefab vào Inspector
    private SkillBarUI currentSkillBar;

    public void ShowSkillBar(HeroUnit hero)
    {
        // Nếu chưa có thì spawn 1 skillbar mới
        if (currentSkillBar == null)
            currentSkillBar = Instantiate(skillBarPrefab, transform);

        currentSkillBar.Setup(hero, hero.GetSkills());
        
        // gán follow theo hero
        WorldSpaceUIFollow follow = currentSkillBar.GetComponent<WorldSpaceUIFollow>();
        if (follow != null)
            follow.target = hero.transform;

        currentSkillBar.Show();
    }

    public void HideSkillBar()
    {
        if (currentSkillBar != null)
            currentSkillBar.Hide();
    }
}

