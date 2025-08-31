using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AssassinateGA>(AssassinatePerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AssassinateGA>();
    }

    private IEnumerator AssassinatePerformer(AssassinateGA assassinateGA)
    {
        Tile tile = GridManager.Instance.GetTileAtPosition(assassinateGA.Target.currentPosition);
        if (tile != null)
        {
            tile.SetUnoccupied(assassinateGA.Target);
        }

        // Có thể chèn animation trước khi destroy
        yield return new WaitForSeconds(0.2f);

        Destroy(assassinateGA.Target.gameObject);
    }

}
