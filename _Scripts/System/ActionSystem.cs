
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;
    public bool IsPerforming {get; private set ;} = false ;
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();
    private static Dictionary<Delegate, Action<GameAction>> _preMap = new(); // Key: original Action<T>, Value: wrapped Action<GameAction>
    private static Dictionary<Delegate, Action<GameAction>> _postMap = new(); //Dictionaries để ánh xạ delegate gốc (Action<T>) tới delegate đã bọc (Action<GameAction>)
    
    public void Perform(GameAction action, Action OnPerformFinished = null)
    {
        if (IsPerforming) 
        {
            return;
        }
        IsPerforming = true ;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false ;
            OnPerformFinished?.Invoke();
        }));
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type , List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            foreach (var sub in subs[type])
            {
                sub(action);
            } 
        }
    }
    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType(); // Lấy loại cụ thể
        if (performers.ContainsKey(type))
        {
            yield return performers[type](action);
        }
    }
    private IEnumerator PerformReactions()
    {
        foreach (var reaction in reactions)
        {
            yield return Flow(reaction);
        }
    }
    public void AddReaction( GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }
    
    public static void AttachPerformer<T> (Func<T, IEnumerator> performer) where T: GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action); // chỗ dấu => là return 
        if (performers.ContainsKey(type)) performers[type] = wrappedPerformer;
        else performers.Add(type, wrappedPerformer);
    }

    public static void DetachPerformer<T>() where T: GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type)) performers.Remove(type);
    }

    public static void SubscriberReaction<T>(Action<T> reaction, ReactionTiming timing) where T: GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Dictionary<Delegate, Action<GameAction>> map = timing == ReactionTiming.PRE ? _preMap : _postMap;

        // Kiểm tra xem delegate 'reaction' này đã được đăng ký (ánh xạ) chưa
        if (map.ContainsKey(reaction))
        {
            return; // Đã đăng ký, không cần làm gì nữa
        }

        // Tạo delegate đã bọc (wrappedReaction) và lưu trữ nó
        Action<GameAction> wrappedReaction = action => reaction((T)action);
        
        if (!subs.ContainsKey(typeof(T)))
        {
            subs.Add(typeof(T), new());
        }
        subs[typeof(T)].Add(wrappedReaction); // Thêm delegate đã bọc vào danh sách subscribers
        map.Add(reaction, wrappedReaction); // Lưu trữ ánh xạ giữa delegate gốc và delegate đã bọc
    }

    public static void UnsubscriberReaction<T>(Action<T> reaction, ReactionTiming timing) where T: GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Dictionary<Delegate, Action<GameAction>> map = timing == ReactionTiming.PRE ? _preMap : _postMap;

        // Chỉ hủy đăng ký nếu kiểu T tồn tại trong danh sách subscribers và delegate gốc đã được ánh xạ
        if (subs.ContainsKey(typeof(T)) && map.ContainsKey(reaction))
        {
            Action<GameAction> wrappedReactionToRemove = map[reaction]; // Lấy đúng instance delegate đã bọc
            subs[typeof(T)].Remove(wrappedReactionToRemove); // Xóa chính xác instance đó khỏi danh sách
            map.Remove(reaction); // Xóa ánh xạ
        }
    }

    public IEnumerator WaitUntilNoPendingActions()
    {
        while (IsPerforming)
        {
            yield return null;
        }
    }
    public IEnumerator PerformAndWait(GameAction action)
    {
        IsPerforming = true;
        yield return StartCoroutine(Flow(action, () => IsPerforming = false));
    }

}
