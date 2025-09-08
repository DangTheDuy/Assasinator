using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
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
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        Button btn = btnObj.GetComponent<Button>();

        btn.onClick.AddListener(() =>
        {
            HeroUnit hero = owner as HeroUnit;
            if (hero == null)
            {
                Debug.LogWarning("Skill owner không phải HeroUnit!");
                return;
            }

            if (!hero.HasEnoughAP(skill.apCost))
            {
                Debug.Log($"Không đủ AP để dùng skill {skill.skillName}");
                return;
            }

            if (skill.requireTarget)
            {
                TargetingSystem.Instance.EnterTargetMode(owner, skill);
            }
            else if (forcedTarget != null)
            {
                skill.Execute(owner, forcedTarget);
            }
            else
            {
                skill.Execute(owner, null);
            }

            hero.SpendAP(skill.apCost);
        });

        btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = skill.skillName;
        btnObj.GetComponentInChildren<Image>().sprite = skill.icon;
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide()
    {
        gameObject.SetActive(false);
        IsEnemyInteractionOpen = false;
    }
}
