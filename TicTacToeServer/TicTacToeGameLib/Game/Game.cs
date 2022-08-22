namespace TicTacToeLib.Game;

public enum Seed : ushort
{
    None = 0,
    Nought = 1,
    Cross = 2
}

public enum GameState : ushort
{
    Inactive = 0,
    Playing = 1,
    Over = 2
}

// This class holds the game state and methods that modify it as game advances.
// You can use this class at server and client side (and even at the same time).
// Server would normall use it to hold single source of truth for every client, to prevent maliciuos
// behaviour fomr clients, etc.
// Server can also serialize game state to DB and restore it as players connecting to resume the game
// Client would use this class to be able to render the game UI according to its state,
// to quickly reflect the game state changes (perform local turns before server confirmation obtained)
// or to run local games (without network connections).
public class Game
{
    public const int GridSizeRows = 3;
    public const int GridSizeColumns = 3;

    public const int UnknownUserId = -1;

    private Seed[,] _grid;
    public Seed[,] Grid { get => _grid; }

    private Dictionary<int, Player> _players;
    public Dictionary<int, Player> Players { get => _players; }

    private GameState _state;
    public GameState State { get => _state; }

    private int _winner;
    public int Winner { get => _winner; }

    private int _activePlayerId;
    public int ActivePlayerId { get => _activePlayerId; }

    // Creates empty game wihout players in inactive state
    public Game()
    {
        _grid = new Seed[GridSizeRows, GridSizeColumns];
        _players = new Dictionary<int, Player>();

        _state = GameState.Inactive;
    }

    // Starts the game. Creates players and assigns them appropriate IDs.
    // Also decides which player plays with X-s and which one with O-s.
    // According to this implementation X always begins the game.
    public void Start(int playerId1, int playerId2)
    {
        _players.Clear();
        for (int i = 0; i < GridSizeRows; i++)
        {
            for (int j = 0; j < GridSizeColumns; j++)
            {
                _grid[i, j] = Seed.None;
            }
        }

        Random rand = new Random();
        int crossPlayer = rand.Next(0, 1);

        Player player1 = new Player();
        player1.Seed = crossPlayer == 0 ? Seed.Cross : Seed.Nought;

        Player player2 = new Player();
        player2.Seed = crossPlayer == 1 ? Seed.Cross : Seed.Nought;

        _players.Add(playerId1, player1);
        _players.Add(playerId2, player2);

        _activePlayerId = crossPlayer == 0 ? playerId1 : playerId2;

        _state = GameState.Playing;
    }

    // Advances game to next state. This method only succeeds if the turn is
    // possible according to the current game state and rules.
    public TurnResult MakeTurn(Turn turn)
    {
        if (_state != GameState.Playing) return new TurnResult(TurnStatus.InvalidTurn);

        if (turn.Seed != _players[_activePlayerId].Seed)
        {
            return new TurnResult(TurnStatus.InvalidTurn);
        }

        if (_grid[turn.PosY, turn.PosX] != Seed.None)
        {
            return new TurnResult(TurnStatus.InvalidPlacement);
        }

        _grid[turn.PosY, turn.PosX] = turn.Seed;
        Seed winnerSeed = FindWinner();
        if (winnerSeed == Seed.None)
        {
            if (EmptyCellExists())
            {
                _activePlayerId = _players.First().Key == _activePlayerId ? _players.Last().Key : _players.First().Key;
            }
            else
            {
                _state = GameState.Over;
                _winner = UnknownUserId;
            }
        }
        else
        {
            _state = GameState.Over;
            _winner = _activePlayerId;
        }

        return new TurnResult(TurnStatus.Success, winnerSeed);
    }

    private Seed FindWinner()
    {
        Seed winnerSeed = CheckWinnerDiagonal();
        if (winnerSeed != Seed.None) return winnerSeed;

        for (int i = 0; i < GridSizeRows; ++i)
        {
            winnerSeed = CheckWinnerRow(i);
            if (winnerSeed != Seed.None) return winnerSeed;
        }

        for (int j = 0; j < GridSizeColumns; ++j)
        {
            winnerSeed = CheckWinnerCol(j);
            if (winnerSeed != Seed.None) return winnerSeed;
        }

        return winnerSeed;
    }

    private bool EmptyCellExists()
    {
        bool emptyCellExists = false;
        for (int i = 0; i < GridSizeRows; ++i)
        {
            for (int j = 0; j < GridSizeColumns; ++j)
            {
                if (_grid[i, j] == Seed.None)
                {
                    emptyCellExists = true;
                    break;
                }
            }
        }

        return emptyCellExists;
    }

    private Seed CheckWinnerRow(int row)
    {
        Seed r = _grid[row, 0];

        if (r != Seed.None)
        {
            for (int j = 0; j < GridSizeColumns; ++j)
            {
                if (_grid[row, j] != r)
                {
                    r = Seed.None;
                    break;
                }
            }
        }        

        return r;
    }

    private Seed CheckWinnerCol(int col)
    {
        Seed r = _grid[0, col];

        if (r != Seed.None)
        {
            for (int i = 0; i < GridSizeRows; ++i)
            {
                if (_grid[i, col] != r)
                {
                    r = Seed.None;
                    break;
                }
            }
        }

        return r;
    }

    private Seed CheckWinnerDiagonal()
    {
        // allow flexibility for grid size
        if (GridSizeColumns != GridSizeRows) return Seed.None;

        Seed leftDiagonal = _grid[0,0];
        Seed rightDiagonal = _grid[0, GridSizeColumns-1];

        for (int k = 0; k < GridSizeColumns; ++k)
        {
            if (_grid[k,k] != leftDiagonal)
            {
                leftDiagonal = Seed.None;
            }
            if (_grid[k, GridSizeColumns - 1 - k] != rightDiagonal)
            {
                rightDiagonal = Seed.None;
            }
        }

        if (leftDiagonal != Seed.None) return leftDiagonal;
        if (rightDiagonal != Seed.None) return rightDiagonal;

        return Seed.None;
    }
}
