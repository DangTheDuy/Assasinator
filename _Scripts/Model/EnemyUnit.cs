
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public int DetectionChance => data.detectionChance;
    private void OnMouseDown()
    {
        if (Unit.SelectedHero == null) return;

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

            skillBar.Setup(this, interactionSkills);
            skillBar.GetComponent<WorldSpaceUIFollow>().target = this.transform;
            skillBar.Show();
        }
    }
}
