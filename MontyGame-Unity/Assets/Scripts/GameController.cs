using UnityEngine;
using MontyGame.Core;
using TMPro;

/// <summary>
/// Main game controller that wires the UI to the MontyGame.Core GameEngine.
/// This is the bridge between user input and core game logic.
/// NO GAME RULES in this script - all rules stay in MontyGame.Core!
/// </summary>
public class GameController : MonoBehaviour
{
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private TextMeshProUGUI positionDisplay;
    [SerializeField] private TextMeshProUGUI turnDisplay;
    [SerializeField] private TextMeshProUGUI storyDisplay;

    private GameEngine gameEngine;
    private BoardManager boardManager;
    private PlayerController playerController;

    void Start()
    {
        // Initialize components
        boardManager = GetComponent<BoardManager>();
        playerController = GetComponent<PlayerController>();

        if (boardManager == null) Debug.LogError("BoardManager not found!");
        if (playerController == null) Debug.LogError("PlayerController not found!");

        // Initialize core game engine
        try
        {
            var world = World1Factory.CreateWorld1();
            gameEngine = new GameEngine(world, new UnityRandomAdapter());
            Debug.Log("✓ GameEngine initialized");
            Debug.Log($"Game started. Players: Dino & Cat");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize GameEngine: {ex.Message}");
        }

        // Wire UI button
        if (rollDiceButton != null)
        {
            rollDiceButton.onClick.AddListener(OnRollDiceClicked);
        }
    }

    public void OnRollDiceClicked()
    {
        if (gameEngine == null) return;

        int roll = gameEngine.RollDice();
        Debug.Log($"[Core] Rolled: {roll}");

        var result = gameEngine.MovePlayer(gameEngine.CurrentPlayerId, roll);
        Debug.Log($"[Core] Player moved to tile {result.NewTile}");

        // Update UI
        UpdateUI();

        // Move player character on screen
        if (playerController != null)
        {
            Vector3 newPosition = boardManager.GetTilePosition(result.NewTile);
            playerController.SetPosition(newPosition);
        }
    }

    void UpdateUI()
    {
        if (gameEngine == null) return;

        // Update position display
        if (positionDisplay != null)
        {
            var playerState = gameEngine.GetPlayerState(gameEngine.CurrentPlayerId);
            positionDisplay.text = $"Tile {playerState.CurrentTile} of 25";
        }

        // Update turn display
        if (turnDisplay != null)
        {
            string playerName = gameEngine.CurrentPlayerId == 0 ? "Dino" : "Cat";
            turnDisplay.text = $"Player: {playerName}";
        }

        // Check win condition
        if (gameEngine.GetCurrentGameState() == GameState.Victory)
        {
            string winner = gameEngine.CurrentPlayerId == 0 ? "Dino" : "Cat";
            if (storyDisplay != null)
            {
                storyDisplay.text = $"🏆 {winner} WINS! 🏆\n\nYou found the Portal Key!\nDino & Cat are going home!";
            }
            Debug.Log($"VICTORY! {winner} wins!");
        }
    }

    public GameEngine GetGameEngine() => gameEngine;
}

/// <summary>
/// Adapter to make UnityEngine's Random work with MontyGame.Core's IRandom interface.
/// </summary>
public class UnityRandomAdapter : IRandom
{
    public int Next(int maxExclusive)
    {
        return UnityEngine.Random.Range(0, maxExclusive);
    }
}
