// File: UIManager.cs (Sửa đổi)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public SkillBarUI skillBarPrefab; 
    private SkillBarUI currentSkillBar;

    public void ShowSkillBar(HeroUnit hero) // Dùng cho Skill Bar mặc định của Hero
    {
        // ... (Giữ nguyên logic cũ nếu cần)
        ShowSkillBarForTarget(hero, null, hero.GetSkills());
    }

    // 🛠️ HÀM MỚI: Hiển thị Skill Bar khi chọn một mục tiêu
    public void ShowSkillBarForTarget(HeroUnit hero, Unit forcedTarget, List<SkillData> skillsToShow)
    {
        if (currentSkillBar == null)
            currentSkillBar = Instantiate(skillBarPrefab, transform);

        // 🚨 SỬ DỤNG HÀM SETUP VỚI FORCED TARGET
        currentSkillBar.Setup(hero, skillsToShow, forcedTarget);
        
        // Gán follow theo Target
        WorldSpaceUIFollow follow = currentSkillBar.GetComponent<WorldSpaceUIFollow>();
        if (follow != null)
        {
            // Follow Enemy nếu có forcedTarget, ngược lại follow Hero
            follow.target = (forcedTarget != null) ? forcedTarget.transform : hero.transform;
        }

        currentSkillBar.Show();
    }

    public void HideSkillBar()
    {
        if (currentSkillBar != null)
            currentSkillBar.Hide();
    }
}