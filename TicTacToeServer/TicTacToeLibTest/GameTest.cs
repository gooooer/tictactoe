using TicTacToeLib.Game;

namespace TicTacToeLibTest;

[TestClass]
public class GameTest
{
    [TestMethod]
    public void TestInactiveAfterInitialization()
    {
        Game game = new Game();
        Assert.IsTrue(game.State == GameState.Inactive);
    }

    [TestMethod]
    public void TestPlayersAreEmpty()
    {
        Game game = new Game();
        Assert.IsTrue(game.Players.Count == 0);
    }

    [TestMethod]
    public void TestMakeTurnNonStartedGame()
    {
        Game game = new Game();
        TurnResult turnResult = game.MakeTurn(new Turn(0, 0, 0, Seed.Cross));
        Assert.IsTrue(turnResult.Status == TurnStatus.InvalidTurn);
    }

    [TestMethod]
    public void TestStartGame()
    {
        Game game = new Game();
        game.Start(0, 1);
        Assert.IsTrue(game.State == GameState.Playing);
        Assert.IsTrue(game.Players.Count == 2);
        Assert.IsTrue(game.Players[game.ActivePlayerId].Seed == Seed.Cross);
    }

    [TestMethod]
    public void TestFirstTurn()
    {
        Game game = new Game();
        game.Start(0, 1);

        TurnResult turnResult = game.MakeTurn(new Turn(game.ActivePlayerId, 1, 1, Seed.Cross));
        Assert.IsTrue(turnResult.Status == TurnStatus.Success);
    }

    [TestMethod]
    public void TestNotYourTurn()
    {
        Game game = new Game();
        game.Start(0, 1);

        TurnResult turnResult = game.MakeTurn(new Turn(game.ActivePlayerId == 0 ? 1 : 0, 1, 1, Seed.Nought));
        Assert.IsTrue(turnResult.Status == TurnStatus.InvalidTurn);
    }

    [TestMethod]
    public void TestNotYourSeed()
    {
        Game game = new Game();
        game.Start(0, 1);

        TurnResult turnResult = game.MakeTurn(new Turn(game.ActivePlayerId, 1, 1, Seed.Nought));
        Assert.IsTrue(turnResult.Status == TurnStatus.InvalidTurn);
    }

    [TestMethod]
    public void TestInvalidPlacement()
    {
        Game game = new Game();
        game.Start(0, 1);

        TurnResult turnResult1 = game.MakeTurn(new Turn(game.ActivePlayerId, 1, 1, Seed.Cross));
        TurnResult turnResult2 = game.MakeTurn(new Turn(game.ActivePlayerId, 1, 1, Seed.Nought));
        Assert.IsTrue(turnResult2.Status == TurnStatus.InvalidPlacement);
    }

    // TODO: test win, loose, draw
}