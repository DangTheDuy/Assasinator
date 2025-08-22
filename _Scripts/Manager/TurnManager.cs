using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int currentTeamTurn { get; private set; } = 0; // 0: Player, 1: Enemy (có thể mở rộng)

    public delegate void TurnChanged(int currentTeam);
    public static event TurnChanged OnTurnChanged;

    public delegate void TurnEnded(); // Định nghĩa delegate mới
    public static event TurnEnded OnTurnEnded; // Định nghĩa event mới

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log($"Lượt bắt đầu: Team {currentTeamTurn} (Player)");
        OnTurnChanged?.Invoke(currentTeamTurn);
    }

    public void EndTurn()
    {
        OnTurnEnded?.Invoke(); // Gọi event khi lượt kết thúc (TRƯỚC khi chuyển lượt)

        currentTeamTurn = 1 - currentTeamTurn; // Chuyển đổi giữa 0 và 1
        Debug.Log($"<color=yellow>Lượt mới: Team {currentTeamTurn} ({(currentTeamTurn == 0 ? "Player" : "Enemy")})</color>");
        OnTurnChanged?.Invoke(currentTeamTurn);


        // Tùy chọn: Thêm logic cho AI của địch ở đây nếu currentTeamTurn là 1
    }

    public bool IsItMyTurn(Unit unit)
    {
        return unit.data.teamID == currentTeamTurn;
    }
}