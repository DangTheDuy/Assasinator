using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AssassinateGA>(AssassinatePerformer);
        ActionSystem.AttachPerformer<ShurikenGA>(ShurikenPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AssassinateGA>();
        ActionSystem.DetachPerformer<ShurikenGA>();
    }

    private IEnumerator AssassinatePerformer(AssassinateGA assassinateGA)
    {
        yield return ExecuteKill(assassinateGA.Target);
        if (Unit.SelectedHero != null)
        {
            UIManager.Instance.ShowSkillBar(Unit.SelectedHero);
        }
    }

    private IEnumerator ShurikenPerformer(ShurikenGA shurikenGA)
    {
        yield return ExecuteKill(shurikenGA.Target);
    }

    private IEnumerator ExecuteKill(Unit target)
    {
        if (target == null)
        {
            Debug.LogWarning("[ExecuteKill] Target null trong ExecuteKill!");
            yield break;
        }

        Debug.Log($"[ExecuteKill] Tiêu diệt {target.name}");
        yield return new WaitForSeconds(0.2f);

        Destroy(target.gameObject);
    }
}
