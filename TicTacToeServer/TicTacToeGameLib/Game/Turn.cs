namespace TicTacToeLib.Game;

// The cornerstone of turn-based game architecture.
// Turn contains all necessary information to let game advance to next state.
// Using turns is also makes it possible to implement replay or undo/redo functionality.
public struct Turn
{
    public readonly int PlayerId;

    public readonly int PosX;
    public readonly int PosY;

    public readonly Seed Seed;

    public Turn(int playerId, int posX, int posY, Seed seed)
    {
        PlayerId = playerId;
        PosX = posX;
        PosY = posY;
        Seed = seed;
    }
}

public enum TurnStatus : ushort
{
    None = 0,
    Success = 1,
    InvalidPlacement = 2,
    InvalidTurn = 100
}

// Result of the attempt to make a turn. Contains all information necessary for
// client to correctly display status and implement logic to process failed and
// successful attempts.
public struct TurnResult
{
    public readonly TurnStatus Status;
    public readonly Seed WinnerSeed;

    public TurnResult(TurnStatus status)
    {
        Status = status;
        WinnerSeed = Seed.None;
    }

    public TurnResult(TurnStatus status, Seed winnerSeed)
    {
        Status = status;
        WinnerSeed = winnerSeed;
    }
}

