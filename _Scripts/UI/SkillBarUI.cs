// File: SkillBarUI.cs (Đã sửa lỗi Setup bị thiếu)
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

    // 🛠️ PHƯƠNG THỨC SETUP ĐÃ ĐƯỢC CHÈN LẠI (Khắc phục lỗi CS1061)
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

        gameObject.SetActive(false);
    }
    // Hết phương thức Setup

    private void CreateSkillButton(SkillData skill)
    {
        HeroUnit hero = owner as HeroUnit;
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        Button btn = btnObj.GetComponent<Button>();
        btnObj.GetComponentInChildren<Image>().sprite = skill.icon;

        bool isInteractable = false;

        // --- BẮT ĐẦU LOGIC KIỂM TRA ĐIỀU KIỆN MỚI ---
        
        // 1. Kiểm tra điều kiện chung: AP và Caster hợp lệ
        if (hero == null || hero.IsDead || !hero.HasEnoughAP(skill.apCost))
        {
            isInteractable = false;
        }
        else
        {
            TargetingData targeting = skill.targeting;

            // 2. Trường hợp Interaction Menu (có forcedTarget - ví dụ: Enemy)
            if (forcedTarget != null)
            {
                // Skill phải có Targeting Data để hoạt động với target bên ngoài
                if (targeting == null)
                {
                    isInteractable = false; // Không thể dùng Skill Self-use cho target khác
                }
                else
                {
                    // 🚨 SỬ DỤNG IsTargetValid ĐỂ KIỂM TRA TÍNH HỢP LỆ
                    isInteractable = targeting.IsTargetValid(hero, forcedTarget.currentPosition);

                    // Xử lý điều kiện đặc biệt (Assassinate)
                    if (skill.skillName == "Assassinate" && hero.IsDetected)
                    {
                        isInteractable = false;
                    }
                }
            }
            // 3. Trường hợp Chọn Target (Targeting Mode) HOẶC Self-use
            else 
            {
                // Nếu Skill cần Target (targeting != null) HOẶC là Self-use (targeting == null)
                // và đủ AP, nó luôn tương tác được để mở Target Mode hoặc Execute ngay.
                isInteractable = true;
            }
        }
        
        // --- Cập nhật nút và Overlay (Giữ nguyên) ---
        btn.interactable = isInteractable;

        Image overlay = btnObj.transform.Find("Overlay")?.GetComponent<Image>();
        if (overlay != null)
        {
            overlay.enabled = !isInteractable;
        }

        // --- Logic onClick (REFACTORED) ---
        btn.onClick.AddListener(() =>
        {
            if (!isInteractable)
            {
                Debug.Log($"Không thể sử dụng skill {skill.skillName} do chưa thỏa điều kiện.");
                return;
            }

            // Đã kiểm tra AP và hero ở trên, chỉ cần kiểm tra trùng lặp
            bool isAlreadyTargetingThisSkill = TargetingSystem.Instance.IsTargeting && selectedSkill == skill;

            if (isAlreadyTargetingThisSkill)
            {
                TargetingSystem.Instance.ExitTargetMode();
                selectedSkill = null;
                Debug.Log($"Đã hủy chọn skill {skill.skillName}");
                return;
            }

            // 🛠️ SỬA LỖI: Xác định có cần TargetMode hay không
            // Chỉ cần mở Targeting Mode nếu Skill có TargetingData VÀ không có ForcedTarget.
            bool requiresTargetingMode = skill.targeting != null && forcedTarget == null;

            if (requiresTargetingMode)
            {
                selectedSkill = skill;
                TargetingSystem.Instance.EnterTargetMode(hero, skill); 
                return;
            }

            // 🛠️ THỰC THI SKILL: Nếu là ForcedTarget hoặc Self-use (không cần Targeting Mode)
            
            Unit targetToExecute = forcedTarget != null ? forcedTarget : owner;
            
            // Xử lý Cost
            hero.SpendAP(skill.apCost);
            
            // 🚨 Gọi logic thực thi Effect
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

    public void ResetSelectedSkill()
    {
        selectedSkill = null;
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide()
    {
        gameObject.SetActive(false);
        IsEnemyInteractionOpen = false;
    }
}