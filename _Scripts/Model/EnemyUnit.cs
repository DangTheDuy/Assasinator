
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public int DetectionChance => data.detectionChance;
    private GameObject highlightOverlay;

    private void OnMouseDown()
    {
        // Nếu đang ở target mode thì giao cho TargetingSystem xử lý
        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsTargeting)
        {
            TargetingSystem.Instance.TrySelectEnemy(this);
            return;
        }

        // Nếu không phải target mode -> xử lý click enemy bình thường
        if (SelectedHero == null) return;

        SelectedEnemy = this;
        Debug.Log($"Enemy {name} được chọn làm target");

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
            highlightOverlay.transform.SetParent(transform);
            highlightOverlay.transform.localPosition = Vector3.zero;
            highlightOverlay.transform.localScale = Vector3.one;

            SpriteRenderer sr = highlightOverlay.AddComponent<SpriteRenderer>();
            sr.sprite = GetComponent<SpriteRenderer>().sprite;
            sr.color = new Color(1f, 0f, 0f, 0.3f); // đỏ mờ
            sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        }

        highlightOverlay.SetActive(active);
    }
}
