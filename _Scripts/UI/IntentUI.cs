using UnityEngine;
using UnityEngine.UI;

public class IntentUI : MonoBehaviour
{
    public static IntentUI Instance;
    [SerializeField] private Image intentImage;
    [SerializeField] private Sprite upArrow;
    [SerializeField] private Sprite downArrow;
    [SerializeField] private Sprite leftArrow;
    [SerializeField] private Sprite rightArrow;

    private void Awake()
    {
        Instance = this;
    }

    public void SetDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) intentImage.sprite = upArrow;
        else if (dir == Vector2Int.down) intentImage.sprite = downArrow;
        else if (dir == Vector2Int.left) intentImage.sprite = leftArrow;
        else if (dir == Vector2Int.right) intentImage.sprite = rightArrow;
    }
}
