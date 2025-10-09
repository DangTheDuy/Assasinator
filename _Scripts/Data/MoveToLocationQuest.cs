using UnityEngine;

[CreateAssetMenu(fileName = "Move to Location Quest", menuName = "Quests/Move to Location")]
public class MoveToLocationQuest : Quest
{
    private Vector2Int targetPosition;

    public void Setup(Vector2Int position)
    {
        targetPosition = position;
        questName = $"Di chuyển đến vị trí ({position.x}, {position.y})";
        description = "Đưa một anh hùng đến ô được đánh dấu để hoàn thành nhiệm vụ.";
    }

    public override void SubscribeToEvents()
    {
        HeroUnit.OnHeroMoved += OnHeroMoved;
    }

    public override void UnsubscribeFromEvents()
    {
        HeroUnit.OnHeroMoved -= OnHeroMoved;
    }

    public override void OnUpdate()
    {
        // Logic cập nhật nếu cần
    }

    private void OnHeroMoved(HeroUnit hero, Vector2Int newPosition)
    {
        if (newPosition == targetPosition)
        {
            CompleteQuest();
        }
    }
}