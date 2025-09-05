using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : Singleton<TargetingSystem>
{
    private SkillData currentSkill;
    private Unit caster;
    public bool IsTargeting => currentSkill != null;

    public void EnterTargetMode(Unit casterUnit, SkillData skill)
    {
        currentSkill = skill;
        caster = casterUnit;

        Vector2Int casterPos = caster.currentPosition;

        foreach (Unit unit in Unit.AllUnits)
        {
            if (unit is EnemyUnit enemy)
            {
                Vector2Int enemyPos = enemy.currentPosition;
                bool isAligned = enemyPos.x == casterPos.x || enemyPos.y == casterPos.y;

                enemy.SetHighlight(isAligned); 
            }
        }
    }


    public void ExitTargetMode()
    {
        currentSkill = null;
        caster = null;

        foreach (Unit unit in Unit.AllUnits)
        {
            if (unit is EnemyUnit enemy)
            {
                enemy.SetHighlight(false);
            }
        }
    }

    public void TrySelectEnemy(EnemyUnit enemy)
    {
        if (currentSkill == null || caster == null || enemy == null) return;

        Debug.Log($"[TargetingSystem] TrySelectEnemy caster={caster?.name} enemy={enemy?.name} currentSkill={currentSkill?.skillName}");

        Vector2Int casterPos = caster.currentPosition;
        Vector2Int enemyPos = enemy.currentPosition;

        bool isAligned = enemyPos.x == casterPos.x || enemyPos.y == casterPos.y;

        if (isAligned)
        {
            currentSkill.Execute(caster, enemy);
            ExitTargetMode();
        }
        else
        {
            Debug.Log($"Enemy {enemy.name} không nằm trong phạm vi kỹ năng!");
        }
    }
}
