using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    public GameObject buttonPrefab; 
    public Transform buttonContainer;

    private Unit owner;
    private Unit forcedTarget; // nếu UI đang hiển thị "trên" 1 unit (ví dụ enemy)

    // Ghi chú: thêm đối số forcedTarget (mặc định null)
    public void Setup(Unit unit, List<SkillData> skills, Unit forcedTarget = null)
    {
        owner = unit;
        this.forcedTarget = forcedTarget;

        Debug.Log($"[SkillBarUI] Setup owner={owner?.name} forcedTarget={forcedTarget?.name} skillsCount={(skills==null?0:skills.Count)}");

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (SkillData skill in skills)
        {
            var s = skill; // capture local để tránh closure bug
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[SkillBarUI] Button clicked: {s.skillName} owner={owner?.name} forcedTarget={this.forcedTarget?.name} requireTarget={s.requireTarget}");
                if (s.requireTarget)
                {
                    TargetingSystem.Instance.EnterTargetMode(owner, s);
                }
                else if (this.forcedTarget != null)
                {
                    // UI đang bám trên 1 unit -> dùng unit đó làm target
                    s.Execute(owner, this.forcedTarget);
                }
                else
                {
                    s.Execute(owner, null);
                }
            });

            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = s.skillName;
            btnObj.GetComponentInChildren<Image>().sprite = s.icon;
        }

        gameObject.SetActive(false);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
