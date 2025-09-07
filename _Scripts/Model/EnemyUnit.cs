
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
    public int DetectionChance => data.detectionChance;
    private GameObject highlightOverlay;

    private void OnMouseDown()
    {
        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsTargeting)
        {
            TargetingSystem.Instance.TrySelectEnemy(this);
            return;
        }

        if (SelectedHero == null) return;

        SkillBarUI skillBar = FindObjectOfType<SkillBarUI>();
        if (skillBar != null)
        {
            List<SkillData> interactionSkills = new List<SkillData>();

            if (!SelectedHero.IsDetected)
            {
                interactionSkills.Add(Resources.Load<SkillData>("Skills/AssassinateSkill"));
            }
            interactionSkills.Add(Resources.Load<SkillData>("Skills/FightSkill"));

            // Owner = hero, forcedTarget = enemy
            skillBar.Setup(SelectedHero, interactionSkills, this);
            skillBar.GetComponent<WorldSpaceUIFollow>().target = this.transform;
            skillBar.Show();
        }
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
    }

}
