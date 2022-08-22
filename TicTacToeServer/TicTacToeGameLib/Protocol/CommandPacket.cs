using TicTacToeLib.Game;
using Utf8Json;

namespace TicTacToeLib.Protocol;

public enum Command : int
{
    JoinGame = 1,
    MakeTurn = 2,
    UpdateGameState = 3,
    EndGame = 4
}

public class CommandPacket
{
    public Command Command;
    public string JsonData;

    public static CommandPacket FromJoinCommand()
    {
        return new CommandPacket() { Command = Command.JoinGame, JsonData = "{}" };
    }

    public static CommandPacket FromGameState(Game.Game game)
    {
        return new CommandPacket { Command = Command.UpdateGameState, JsonData = JsonSerializer.ToJsonString(game) };
    }

    public static CommandPacket FromTurn(Turn turn)
    {
        return new CommandPacket { Command = Command.MakeTurn, JsonData = JsonSerializer.ToJsonString(turn) };
    }
}
