using TicTacToeLib.Game;

// Abstraction layer for remote communication with the server.
public interface IGameSession
{
    public void MakeTurn(Turn turn);
}
