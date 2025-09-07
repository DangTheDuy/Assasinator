using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    public GameObject buttonPrefab; 
    public Transform buttonContainer;

    private Unit owner;
    private Unit forcedTarget;
    public static bool IsEnemyInteractionOpen { get; private set; } = false;


    public void Setup(Unit unit, List<SkillData> skills, Unit forcedTarget = null)
    {
        owner = unit;
        this.forcedTarget = forcedTarget;
        IsEnemyInteractionOpen = forcedTarget != null;

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (SkillData skill in skills)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (skill.requireTarget)
                {
                    TargetingSystem.Instance.EnterTargetMode(owner, skill);
                }
                else if (this.forcedTarget != null)
                {
                    skill.Execute(owner, this.forcedTarget);
                }
                else
                {
                    skill.Execute(owner, null);
                }
            });

            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = skill.skillName;
            btnObj.GetComponentInChildren<Image>().sprite = skill.icon;
        }

        gameObject.SetActive(false);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide()
    {
        gameObject.SetActive(false);
        IsEnemyInteractionOpen = false;
    }
}
