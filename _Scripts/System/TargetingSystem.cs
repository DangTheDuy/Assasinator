using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : Singleton<TargetingSystem>
{
    private SkillData currentSkill;
    private ItemStack currentItemStack;
    private Unit caster;
    public bool IsTargeting => currentSkill != null;

    public void EnterTargetMode(Unit casterUnit, SkillData skill, ItemStack stack = null)
    {
        currentSkill = skill;
        currentItemStack = stack;
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

        Vector2Int casterPos = caster.currentPosition;
        Vector2Int enemyPos = enemy.currentPosition;

        bool isAligned = enemyPos.x == casterPos.x || enemyPos.y == casterPos.y;

        if (isAligned)
        {
            currentSkill.Execute(caster, enemy);
            if (caster is HeroUnit hero)
            {
                if (currentItemStack != null)
                {
                    if (!currentItemStack.Consume())
                    {
                        Debug.Log($"{hero.name} đã hết {currentItemStack.itemData.itemName}!");
                    }
                    else
                    {
                        Debug.Log($"{hero.name} dùng {currentItemStack.itemData.itemName}, còn lại {currentItemStack.quantity}");
                        if (currentItemStack.quantity <= 0)
                            hero.inventory.Remove(currentItemStack);

                        HeroHUDManager hudManager = FindObjectOfType<HeroHUDManager>();
                        if (hudManager != null)
                            hudManager.UpdateHeroItems(hero, hero.inventory);
                    }
                }
                hero.SpendAP(currentSkill.apCost);
            }
            ExitTargetMode();
            SkillBarUI.Instance?.ResetSelectedSkill();
        }
    }
}
