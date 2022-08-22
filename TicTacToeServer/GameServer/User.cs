using TicTacToeLib.Game;
namespace GameServer;

public class User
{
    public int AssignedPlayerId { get; set; }

    public User()
    {
        AssignedPlayerId = Game.UnknownUserId;
    }
}

