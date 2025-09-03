using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    public GameObject buttonPrefab; 
    public Transform buttonContainer;

    private Unit owner;

    public void Setup(Unit unit, List<SkillData> skills)
    {
        owner = unit;
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
                    if (Unit.SelectedEnemy != null)
                        skill.Execute(owner, Unit.SelectedEnemy);
                    else
                        Debug.LogWarning($"Skill '{skill.skillName}' yêu cầu target nhưng chưa chọn enemy!");
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
    public void Hide() => gameObject.SetActive(false);
}
