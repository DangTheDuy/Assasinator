using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Drag Settings")]
    public float dragSpeed = 2f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    [Header("Padding Around Map")]
    public float paddingLeft = 20f;
    public float paddingBottom = 20f;
    public float paddingRight = 20f;
    public float paddingTop = 20f;

    private Camera cam;
    private Vector3 dragOrigin;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    void LateUpdate()
    {
        ClampCameraPosition();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += difference;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, GetMaxZoomBasedOnMap());
        }
    }

    float GetMaxZoomBasedOnMap()
    {
        if (GridManager.Instance == null || GridManager.Instance.tiles.Count == 0) return maxZoom;

        Vector2Int min = GridManager.Instance.MapMin;
        Vector2Int max = GridManager.Instance.MapMax;
        float tileSize = GridManager.Instance.TileSize;

        float mapWidth = (max.x - min.x + 1) * tileSize + paddingLeft;
        float mapHeight = (max.y - min.y + 1) * tileSize + + paddingBottom;

        float zoomByWidth = mapWidth / (2f * cam.aspect);
        float zoomByHeight = mapHeight / 2f;

        return Mathf.Clamp(Mathf.Min(zoomByWidth, zoomByHeight), minZoom, maxZoom);
    }

    void ClampCameraPosition()
    {
        if (GridManager.Instance == null || GridManager.Instance.tiles.Count == 0) return;

        Vector2Int min = GridManager.Instance.MapMin;
        Vector2Int max = GridManager.Instance.MapMax;
        float tileSize = GridManager.Instance.TileSize;

        float camHalfWidth = cam.orthographicSize * cam.aspect;
        float camHalfHeight = cam.orthographicSize;

        float minX = (min.x * tileSize) - paddingLeft + camHalfWidth;
    float maxX = ((max.x + 1) * tileSize) + paddingRight - camHalfWidth;
    float minY = (min.y * tileSize) - paddingBottom + camHalfHeight;
    float maxY = ((max.y + 1) * tileSize) + paddingTop - camHalfHeight;

        Vector3 clamped = cam.transform.position;
        clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
        clamped.y = Mathf.Clamp(clamped.y, minY, maxY);
        cam.transform.position = clamped;
    }
}
