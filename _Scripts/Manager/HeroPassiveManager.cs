// File: HeroPassiveManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class HeroPassiveManager : MonoBehaviour
{
    private HeroUnit hero;
    private List<PassiveData> activePassives = new List<PassiveData>();
    private readonly Dictionary<PassiveData, Action<GameAction>> _wrappedReactionMap = new();


    public void Setup(HeroUnit caster, List<PassiveData> passives)
    {
        hero = caster;
        activePassives = passives;
        Debug.Log($"PassiveManager của {hero.name} đã nhận {passives.Count} passive.");
        
        CleanupExistingSubscriptions();
        RegisterPassives();
    }

    private void OnDestroy()
    {
        CleanupExistingSubscriptions();
    }

    private void CleanupExistingSubscriptions()
    {
        foreach (var kvp in _wrappedReactionMap)
        {
            PassiveData passive = kvp.Key;
            Action<GameAction> wrappedAction = kvp.Value;
            
            Type actionType = GetActionTypeFromTrigger(passive.triggerType);

            if (actionType != null)
            {
                var method = typeof(ActionSystem).GetMethod(nameof(ActionSystem.UnsubscriberReaction));
                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(actionType);
                }
            }
        }
        _wrappedReactionMap.Clear();
    }
    
    private void RegisterPassives()
    {
        foreach (PassiveData passive in activePassives)
        {
            if (passive.passiveLogic == null) continue;
            
            Type actionType = GetActionTypeFromTrigger(passive.triggerType);
            if (actionType == null) continue;
            
            Action<GameAction> passiveHandler = action =>
            {
                // Chỉ kích hoạt nếu Hero này là người thực hiện Action (giả định Action có thuộc tính Caster)
                if (action.Caster != hero) return;
                passive.passiveLogic.ApplyPassiveEffect(action); 
            };

            // 2. Sử dụng Reflection để gọi ActionSystem.SubscriberReaction<T>(...)
            var method = typeof(ActionSystem).GetMethod(nameof(ActionSystem.SubscriberReaction));
            if (method == null) continue;

            var genericMethod = method.MakeGenericMethod(actionType);
            Action<GameAction> genericReaction = action => passiveHandler(action);
            Delegate originalReactionDelegate = CreateDelegate(actionType, passiveHandler);
            
            if (originalReactionDelegate != null)
            {
                genericMethod.Invoke(null, new object[] { originalReactionDelegate, ReactionTiming.POST });
                _wrappedReactionMap.Add(passive, passiveHandler); 
            }
        }
    }

    private Delegate CreateDelegate(Type actionType, Action<GameAction> handler)
    {      
        var actionTypeGeneric = typeof(Action<>).MakeGenericType(actionType);
        return handler; 
    }

    private Type GetActionTypeFromTrigger(PassiveTriggerType trigger)
    {
        switch (trigger)
        {
            case PassiveTriggerType.OnAssassinateCompleted:
                return typeof(AssassinateGA); 
            case PassiveTriggerType.OnActionCompleted:
                return typeof(GameAction);
            default:
                return null;
        }
    }
}