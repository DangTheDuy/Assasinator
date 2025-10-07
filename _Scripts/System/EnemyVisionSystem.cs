using System.Collections.Generic;
using UnityEngine;

public class EnemyVisionDisplaySystem : MonoBehaviour
{
    public static EnemyVisionDisplaySystem Instance;

    private readonly Dictionary<Tile, GameObject> overlays = new();
    private bool isVisible = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleEnemyVision();
        }
    }

    // ======================================== TOGGLE DISPLAY ========================================
    public void ToggleEnemyVision()
    {
        isVisible = !isVisible;
        if (isVisible)
            ShowEnemyVision();
        else
            HideAllOverlays();
    }

    // ======================================== SHOW ENEMY VISION ========================================
    private void ShowEnemyVision()
    {
        HideAllOverlays();

        var enemies = FindObjectsOfType<EnemyUnit>();
        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            int visionRange = Mathf.Max(0, enemy.visionRange); 
            Vector2Int pos = enemy.currentPosition;

            List<Vector2Int> cellsInRange = GridManager.Instance.GetCellsInDiamondRange(pos, visionRange);
            foreach (var cell in cellsInRange)
            {
                Tile tile = GridManager.Instance.GetTileAtPosition(cell);
                if (tile == null || !tile.IsVisible) continue; 

                // Nếu tile chưa có overlay thì tạo
                if (!overlays.ContainsKey(tile))
                {
                    GameObject overlay = new GameObject("EnemyVisionOverlay");
                    overlay.transform.SetParent(tile.transform, false);
                    overlay.transform.localPosition = new Vector3(0, 0, -0.05f);

                    SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
                    SpriteRenderer baseSR = tile.GetComponent<SpriteRenderer>();

                    if (baseSR != null)
                    {
                        sr.sprite = baseSR.sprite;
                        sr.sortingLayerID = baseSR.sortingLayerID;
                        sr.sortingOrder = baseSR.sortingOrder + 1;
                    }

                    // ✅ màu đỏ mờ để biểu thị vùng tầm nhìn enemy
                    sr.color = new Color(1f, 0f, 0f, 0.25f);
                    overlays[tile] = overlay;
                }
            }
        }
    }

    // ======================================== HIDE ALL OVERLAYS ========================================
    public void HideAllOverlays()
    {
        foreach (var kv in overlays)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }
        overlays.Clear();
    }
}
