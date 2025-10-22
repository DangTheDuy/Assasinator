using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Skill System/Passive Effect/Double Assassinate")]
public class DoubleAssassinate : PassiveEffectData
{
    public override void ApplyPassiveEffect(GameAction triggerAction)
    {
        if (!(triggerAction is AssassinateGA assassinateAction)) return;

        if (assassinateAction.IsPassiveAction) 
        {
            return; // Dừng, chỉ kích hoạt một cấp độ (cấp 1)
        }
        
        Unit caster = assassinateAction.Caster; 
        Unit originalTarget = assassinateAction.Target; // Không cast ngay để lấy vị trí an toàn
        
        // 1. Lấy vị trí của sự kiện (ngay cả khi mục tiêu đã bị hủy)
        Vector2Int assassinationPosition = originalTarget.currentPosition;
        
        // 2. Kiểm tra tính hợp lệ cơ bản
        if (caster == null || !(originalTarget is EnemyUnit) ) return;
        
        // 🚨 SỬA LỖI LOGIC: Kẻ địch thứ hai phải ở cùng vị trí với vụ ám sát.
        Debug.Log($"[PASSIVE DOUBLE] Tìm kiếm tại vị trí: {assassinationPosition}. Tổng Units: {Unit.AllUnits.Count}");

        // Mục tiêu thứ hai phải còn sống (không IsDead) và ở cùng vị trí
        EnemyUnit secondaryTarget = Unit.AllUnits
            .OfType<EnemyUnit>()
            // 1. Phải ở cùng vị trí VỤ ÁM SÁT
            .Where(enemy => enemy.currentPosition == assassinationPosition) 
            // 2. KHÔNG phải chính xác mục tiêu ban đầu (tránh lỗi)
            .Where(enemy => enemy != originalTarget) 
            // 3. Phải còn sống (rất quan trọng)
            .Where(enemy => !enemy.IsDead) 
            .OrderBy(_ => Random.value) 
            .FirstOrDefault(); 

        if (secondaryTarget != null)
        {
            // 🚨 TẠO ACTION BỊ ĐỘNG VÀ ĐẶT CỜ
            AssassinateGA secondaryAction = new AssassinateGA(caster, secondaryTarget, isPassive: true); 

            // Thêm vào PostReactions để được thực thi ngay sau Action gốc
            triggerAction.PostReactions.Add(secondaryAction); 

            Debug.Log($"[PASSIVE DOUBLE] KÍCH HOẠT: Am sát thêm {secondaryTarget.name}.");
        } else {
            Debug.Log($"[PASSIVE DOUBLE] Thất bại: Không tìm thấy mục tiêu thứ hai cùng ô.");
        }
    }
}