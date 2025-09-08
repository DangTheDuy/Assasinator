
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
    public int DetectionChance => data.detectionChance;
    private GameObject highlightOverlay;
    private GameObject arrowInstance;

    private void OnMouseDown()
    {
        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsTargeting)
        {
            TargetingSystem.Instance.TrySelectEnemy(this);
            return;
        }

        if (SelectedHero == null) return;

        SkillBarUI skillBar = FindObjectOfType<SkillBarUI>();
        if (skillBar == null) return;

        // Nếu đã chọn enemy này rồi → hủy chọn enemy, hiện lại skill bar của Hero
        if (SelectedEnemy == this)
        {
            SelectedEnemy = null;
            skillBar.Hide();

            // Hiện lại skill bar của Hero
            skillBar.Setup(SelectedHero, SelectedHero.GetSkills(), null);
            skillBar.GetComponent<WorldSpaceUIFollow>().target = SelectedHero.transform;
            skillBar.Show();

            return;
        }

        // Nếu chọn enemy mới
        SelectedEnemy = this;
        List<SkillData> interactionSkills = new List<SkillData>();

        if (!SelectedHero.IsDetected && SelectedHero.currentPosition == currentPosition)
        {
            interactionSkills.Add(Resources.Load<SkillData>("Skills/AssassinateSkill"));
        }
        interactionSkills.Add(Resources.Load<SkillData>("Skills/FightSkill"));

        skillBar.Setup(SelectedHero, interactionSkills, this);
        skillBar.GetComponent<WorldSpaceUIFollow>().target = this.transform;
        skillBar.Show();
    }


    public void SetHighlight(bool active)
    {
        if (highlightOverlay == null)
        {
            highlightOverlay = new GameObject("HighlightOverlay");
            highlightOverlay.transform.SetParent(icon.transform); 
            highlightOverlay.transform.localPosition = Vector3.zero;
            highlightOverlay.transform.localScale = Vector3.one;

            Image overlayImage = highlightOverlay.AddComponent<Image>();
            overlayImage.sprite = icon.sprite; 
            overlayImage.color = new Color(1f, 0f, 0f, 0.5f); 
            overlayImage.raycastTarget = false; 

            highlightOverlay.transform.SetSiblingIndex(icon.transform.GetSiblingIndex() + 1);
        }

        highlightOverlay.SetActive(active);

        // ✅ Hiện hoặc ẩn arrow dùng ArrowFollowUnit
        if (active)
        {
            if (arrowInstance == null)
            {
                arrowInstance = Instantiate(Resources.Load<GameObject>("Prefabs/ArrowUI"));
                arrowInstance.transform.Find("ArrowContainer").GetComponent<ArrowFollowUnit>().target = transform;
            }

            arrowInstance.SetActive(true);
        }
        else
        {
            if (arrowInstance != null)
                arrowInstance.SetActive(false);
        }
    }
}
