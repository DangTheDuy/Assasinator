// File: SkillBarUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : Singleton<SkillBarUI>
{
    public GameObject buttonPrefab; 
    public Transform buttonContainer;
    private Unit owner;
    private Unit forcedTarget;
    private SkillData selectedSkill = null;
    public static bool IsEnemyInteractionOpen { get; private set; } = false;

    public void Setup(Unit unit, List<SkillData> skills, Unit forcedTarget = null)
    {
        owner = unit;
        this.forcedTarget = forcedTarget;
        IsEnemyInteractionOpen = forcedTarget != null;

        ClearButtons();

        foreach (SkillData skill in skills)
        {
            CreateSkillButton(skill);
        }

        // Tạm thời ẩn đi để UIManager.Show/Hide kiểm soát
        // gameObject.SetActive(false); 
        // 🚨 Lưu ý: Dòng này đã được di chuyển ra ngoài UIManager để kiểm soát
        if (forcedTarget == null)
        {
            gameObject.SetActive(false); // Chỉ ẩn nếu là skill bar của Hero (mặc định)
        }
    }

    private void CreateSkillButton(SkillData skill)
    {
        HeroUnit hero = owner as HeroUnit;
        
        // --- KIỂM TRA NULL AN TOÀN HƠN CHO NRE (Dòng 38 cũ) ---
        if (buttonPrefab == null || buttonContainer == null)
        {
            Debug.LogError("SkillBarUI: buttonPrefab hoặc buttonContainer bị thiếu tham chiếu trong Inspector!");
            return;
        }

        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        Button btn = btnObj.GetComponent<Button>();
        
        Image skillIcon = btnObj.GetComponentInChildren<Image>();
        if (skillIcon != null && skill.icon != null)
        {
            skillIcon.sprite = skill.icon;
        } 
        else if (skillIcon == null)
        {
            // Lỗi NRE xảy ra nếu Image không tồn tại
            Debug.LogWarning($"SkillBarUI: Button prefab '{buttonPrefab.name}' thiếu component Image con.");
            return;
        }
        // --------------------------------------------------------

        bool isInteractable = false;
        
        // ... (Giữ nguyên LOGIC KIỂM TRA ĐIỀU KIỆN MỚI) ...
        if (hero == null || hero.IsDead || !hero.HasEnoughAP(skill.apCost))
        {
            isInteractable = false;
        }
        else
        {
            TargetingData targeting = skill.targeting;
            if (forcedTarget != null)
            {
                if (targeting == null)
                {
                    isInteractable = false; 
                }
                else
                {
                    isInteractable = targeting.IsTargetValid(hero, forcedTarget.currentPosition);
                    if (skill.skillName == "Assassinate" && hero.IsDetected)
                    {
                        isInteractable = false;
                    }
                }
            }
            else 
            {
                isInteractable = true;
            }
        }
        
        btn.interactable = isInteractable;
        Image overlay = btnObj.transform.Find("Overlay")?.GetComponent<Image>();
        if (overlay != null)
        {
            overlay.enabled = !isInteractable;
        }

        // ... (Giữ nguyên LOGIC ONCLICK REFACTORED) ...
        btn.onClick.AddListener(() =>
        {
            if (!isInteractable)
            {
                Debug.Log($"Không thể sử dụng skill {skill.skillName} do chưa thỏa điều kiện.");
                return;
            }

            bool isAlreadyTargetingThisSkill = TargetingSystem.Instance.IsTargeting && selectedSkill == skill;

            if (isAlreadyTargetingThisSkill)
            {
                TargetingSystem.Instance.ExitTargetMode();
                selectedSkill = null;
                Debug.Log($"Đã hủy chọn skill {skill.skillName}");
                return;
            }

            bool requiresTargetingMode = skill.targeting != null && forcedTarget == null;

            if (requiresTargetingMode)
            {
                selectedSkill = skill;
                TargetingSystem.Instance.EnterTargetMode(hero, skill); 
                return;
            }
            
            Unit targetToExecute = forcedTarget != null ? forcedTarget : owner;
            
            hero.SpendAP(skill.apCost);
            
            foreach (var effect in skill.effects)
            {
                GameAction action = effect.CreateAction(owner, targetToExecute);
                if (action != null)
                    ActionSystem.Instance.Perform(action);
            }

            ResetSelectedSkill();
        });
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);
    }
    
    public void ResetSelectedSkill() { selectedSkill = null; }
    public void Show() => gameObject.SetActive(true);
    public void Hide()
    {
        gameObject.SetActive(false);
        IsEnemyInteractionOpen = false;
    }
}