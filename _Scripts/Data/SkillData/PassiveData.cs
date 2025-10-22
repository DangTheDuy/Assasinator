using UnityEngine;
using System.Collections.Generic;

public enum PassiveTriggerType 
{ 
    OnActionCompleted,         // Kích hoạt khi một GameAction bất kỳ hoàn thành
    OnAssassinateCompleted,    // Kích hoạt sau khi AssassinateGA hoàn thành
    OnDamageDealt,             // Kích hoạt khi gây sát thương
    OnKillConfirmed            // Kích hoạt khi một Unit chết
    //... thêm các loại sự kiện khác
}

[CreateAssetMenu(menuName = "Skill System/Passive Skill Data (base)")]
public class PassiveData : ScriptableObject
{
    public string passiveName;
    public PassiveTriggerType triggerType;
    
    [Tooltip("Logic Scriptable Object sẽ được áp dụng khi TriggerType xảy ra")]
    public PassiveEffectData passiveLogic;
}