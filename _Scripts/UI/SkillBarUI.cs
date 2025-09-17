using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : Singleton<SkillBarUI>
{
    public GameObject buttonPrefab; 
    public Transform buttonContainer;
    private Unit owner;
    private Unit forcedTarget;
    private SkillData selectedSkill = null;
    public static bool IsEnemyInteractionOpen { get; private set; } = false;

    public void Setup(Unit unit, List<SkillData> skills, Unit forcedTarget = null)
    {
        owner = unit;
        this.forcedTarget = forcedTarget;
        IsEnemyInteractionOpen = forcedTarget != null;

        ClearButtons();

        foreach (SkillData skill in skills)
        {
            CreateSkillButton(skill);
        }

        gameObject.SetActive(false);
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);
    }

    private void CreateSkillButton(SkillData skill)
    {
        HeroUnit hero = owner as HeroUnit;
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        Button btn = btnObj.GetComponent<Button>();
        btnObj.GetComponentInChildren<Image>().sprite = skill.icon;

        bool isInteractable = false;

                if (skill.skillName == "Fight")
                {
                    isInteractable = hero != null && forcedTarget != null &&
                            hero.currentPosition == forcedTarget.currentPosition
                            && hero.HasEnoughAP(skill.apCost);;
                }
                else if (skill.skillName == "Assassinate")
                {
                    isInteractable = hero != null && forcedTarget != null &&
                                    !hero.IsDetected &&
                                    hero.currentPosition == forcedTarget.currentPosition
                                    && hero.HasEnoughAP(skill.apCost);
                }
                else
                {
                    isInteractable = hero != null && hero.HasEnoughAP(skill.apCost);
                }

        btn.interactable = isInteractable;

        Image overlay = btnObj.transform.Find("Overlay")?.GetComponent<Image>();
        if (overlay != null)
        {
            overlay.enabled = !isInteractable;
        }

        btn.onClick.AddListener(() =>
        {
            if (!isInteractable)
            {
                Debug.Log($"Không thể sử dụng skill {skill.skillName} do chưa thỏa điều kiện.");
                return;
            }

            if (hero == null)
            {
                Debug.LogWarning("Skill owner không phải HeroUnit!");
                return;
            }

            bool isAlreadyTargetingThisSkill = TargetingSystem.Instance.IsTargeting && selectedSkill == skill;
            bool notEnoughAP = !hero.HasEnoughAP(skill.apCost);

            if (isAlreadyTargetingThisSkill || notEnoughAP)
            {
                if (isAlreadyTargetingThisSkill)
                {
                    TargetingSystem.Instance.ExitTargetMode();
                    selectedSkill = null;
                    Debug.Log($"Đã hủy chọn skill {skill.skillName}");
                }
                return;
            }

            if (skill.requireTarget)
            {
                selectedSkill = skill;
                TargetingSystem.Instance.EnterTargetMode(owner, skill);
                return;
            }

            skill.Execute(owner, forcedTarget);
            hero.SpendAP(skill.apCost);
            ResetSelectedSkill();
        });
    }


    public void ResetSelectedSkill()
    {
        selectedSkill = null;
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide()
    {
        gameObject.SetActive(false);
        IsEnemyInteractionOpen = false;
    }
}
