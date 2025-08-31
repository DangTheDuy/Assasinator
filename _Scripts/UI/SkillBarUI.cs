using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    public GameObject buttonPrefab; // Prefab 1 nút skill (có Image + Text)
    public Transform buttonContainer;

    private Unit owner;

    public void Setup(Unit unit, List<SkillData> skills)
    {
        owner = unit;

        // Clear các nút cũ nếu có
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Tạo nút cho mỗi skill
        foreach (SkillData skill in skills)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
           btn.onClick.AddListener(() =>
            {
                if (Unit.SelectedEnemy != null)
                    skill.Execute(owner, Unit.SelectedEnemy);
                else
                    Debug.LogWarning("Chưa chọn enemy target!");
            });

            // set icon/text nếu có
            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = skill.skillName;
            btnObj.GetComponentInChildren<Image>().sprite = skill.icon;
        }

        gameObject.SetActive(false); // ẩn mặc định
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
