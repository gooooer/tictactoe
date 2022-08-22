using TicTacToeLib.Game;

// This piece abstracts implementation of commands received from server.
public interface IBoardRemoteControl
{
    public void AssignPlayerId(int playerId);

    public void ApplyGameStateFromServer(Game game);

    public void ApplyTurnFromServer(Turn turn);   
}
