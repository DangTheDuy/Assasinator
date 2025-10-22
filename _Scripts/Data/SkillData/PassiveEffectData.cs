using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveEffectData : ScriptableObject
{
    // Hàm này sẽ được gọi khi PassiveManager phát hiện sự kiện phù hợp.
    // triggerAction: Hành động đã kích hoạt passive (ví dụ: AssassinateGA, DamageGA).
    // caster: Hero sở hữu passive.
    // target: Mục tiêu của hành động kích hoạt.
    public abstract void ApplyPassiveEffect(GameAction triggerAction);
}
