using UnityEngine;
using System;

public enum GameState
{
    None,
    SignIn,
    MainMenu,
    Lobby,
    Connecting,
    GameSelection,
    InGame,
    Loading,
    Result,
    Disconnected
}

public enum GameName
{
    None,
    RollingDice,
    RacingGame,
    MiniGame2,
    Racing,
    Puzzle,
    Shooter
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Info")]
    [SerializeField] private GameName currentGame = GameName.None;
    [SerializeField] private GameState currentState = GameState.None;

    public GameName CurrentGame => currentGame;
    public GameState CurrentState => currentState;

    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetGame(GameName game)
    {
        currentGame = game;
        Debug.Log($"Game set to: {game}");
    }

    public void SetState(GameState state)
    {
        if (currentState != state)
        {
            currentState = state;
            Debug.Log($"Game State changed to: {state}");
            OnGameStateChanged?.Invoke(state);
        }
    }

    // You can add more utility functions here
    public bool IsInGame()
    {
        return currentState == GameState.InGame;
    }

    public void ResetGameManager()
    {
        currentGame = GameName.None;
        currentState = GameState.None;
    }
}
