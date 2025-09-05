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

        Debug.Log($"[ExecuteKill] Bắt đầu tiêu diệt: {target.name} id={target.GetInstanceID()} pos={target.currentPosition} active={target.gameObject.activeInHierarchy}");

        // cố gắng remove khỏi tile nếu có
        if (GridManager.Instance != null)
        {
            Tile tile = GridManager.Instance.GetTileAtPosition(target.currentPosition);
            if (tile != null)
            {
                Debug.Log($"[ExecuteKill] Remove {target.name} khỏi tile {tile.gridPosition}");
                tile.SetUnoccupied(target);
            }
            else
            {
                Debug.Log($"[ExecuteKill] Không tìm thấy tile tại {target.currentPosition} để remove {target.name}");
            }
        }

        // remove khỏi list global + selection (đề phòng OnDestroy không chạy kịp)
        if (Unit.AllUnits.Contains(target))
        {
            Unit.AllUnits.Remove(target);
            Debug.Log($"[ExecuteKill] Remove {target.name} khỏi Unit.AllUnits");
        }

        if (Unit.SelectedEnemy == target) Unit.SelectedEnemy = null;
        if (Unit.SelectedHero == target) Unit.SelectedHero = null;

        // ẩn ngay lập tức để khỏi thấy trên màn hình
        target.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        Destroy(target.gameObject);
        Debug.Log($"[ExecuteKill] Destroy called cho {target.name}");
    }


}
