using TicTacToeLib.Game;
using TMPro;
using UnityEngine;

// This class represents the main operating area for the game.
// It holds its copy of the game state and applies game state-changing operations
// from the server. It also takes user input and sends game commands (Turns) to the server.
public class Board : MonoBehaviour, IBoardRemoteControl
{
    // Unity Editor connections
    public GameObject CellPrefab;
    public GameObject NetworkClient;
    public TMP_Text MessageText;

    // Board operating data
    private const int Rows = 3;
    private const int Cols = 3;
    private const float CellOffset = 2.5f;

    private MessageToaster _messageToaster;
    
    private Cell[,] _cells = new Cell[Rows, Cols];

    private int _myPlayerId = Game.UnknownUserId;
    private Game _game = new Game();
    private IGameSession _gameSession;

    // Unity component lifecycle

    void Start()
    {
        _messageToaster = new MessageToaster(MessageText);
        // Only take game session from network client. Don't need other stuff.
        _gameSession = NetworkClient.GetComponent<NetworkClient>();        
        CreateCells();
    }

    void Update()
    {
        _messageToaster.Update();
        lock (_game)
        {
            UpdateCellsFromGame();
        }
    }

    // IBoardRemoteControl

    public void AssignPlayerId(int playerId)
    {
        Debug.Log("Got player Id: " + playerId);
        _myPlayerId = playerId;

        _messageToaster.ShowText("You've joined the game!");
    }

    public void ApplyGameStateFromServer(Game game)
    {
        // Reference assignment is guaranteed to be atomic - no need to worry about locking the value
        _game = game;

        if (game.Players.ContainsKey(_myPlayerId))
        {
            string playerSeedName = game.Players[_myPlayerId].Seed == Seed.Nought ? "Nought (0)" : "Cross (X)";

            _messageToaster.ShowText("Game started! Your seed is " + playerSeedName);
        }
        else
        {
            _messageToaster.ShowText("You are spectator.");
        }
    }

    public void ApplyTurnFromServer(Turn turn)
    {
        // We need to lock game object mutation since this function is called from another thread.
        // Because we know performing a turn does not take much time, we can lock the
        // entire game object (therefore UI thread) for a nanosec.
        lock (_game)
        {
            _game.MakeTurn(turn);
            if (_game.State == GameState.Over)
            {
                _messageToaster.ShowText(GetGameOverMessage());
            }
        }
    }

    // Internal

    // Handling user input
    private void OnCellClick(Cell cell)
    {
        if (_game.State == GameState.Playing && _game.ActivePlayerId == _myPlayerId)
        {
            Turn turn = new Turn(_myPlayerId, cell.Col, Rows - 1 - cell.Row, _game.Players[_myPlayerId].Seed);
            _gameSession.MakeTurn(turn);
        }
        else
        {
            switch (_game.State)
            {
                case GameState.Inactive:
                    {
                        _messageToaster.ShowText("Waiting for other players");
                        break;
                    }
                case GameState.Playing:
                    {
                        _messageToaster.ShowText("It's not your turn!");
                        break;
                    }
            }
        }
    }

    private void CreateCells()
    {
        Transform container = (new GameObject("Cells")).transform;

        container.transform.parent = transform;
        container.transform.localPosition = new Vector3(-3f, 0.01f, -3f);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                GameObject go = (GameObject)Instantiate(CellPrefab, Vector3.zero, Quaternion.identity);
                Cell cell = go.GetComponent<Cell>();

                cell.transform.parent = container;
                cell.transform.localPosition = new Vector3(col * CellOffset, row * CellOffset, 0f);

                cell.Init(row, col, OnCellClick);

                _cells[row, col] = cell;
            }
        }
    }

    private void UpdateCellsFromGame()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                _cells[Rows - 1 - row, col].SetSeed(_game.Grid[row, col]);
            }
        }
    }

    private string GetGameOverMessage()
    {
        if (_game.Winner == Game.UnknownUserId)
            return "Neither side won";
        else if (_game.Winner == _myPlayerId)
            return "Congrats! You won!";
        else if (!_game.Players.ContainsKey(_myPlayerId))
            return _game.Players[_game.Winner].Seed + " won!";
        else
            return "Game over! You lost!";        
    }
}
