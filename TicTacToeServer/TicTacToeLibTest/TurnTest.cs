using TicTacToeLib.Game;

namespace TicTacToeLibTest;

[TestClass]
public class TurnTest
{
    [TestMethod]
    public void TestDefaultTurn()
    {
        Turn turn = new Turn(0, 1, 1, Seed.Cross);
        Assert.AreEqual(turn.PlayerId, 0);
        Assert.AreEqual(turn.PosX, 1);
        Assert.AreEqual(turn.PosY, 1);
        Assert.AreEqual(turn.Seed, Seed.Cross);
    }

    [TestMethod]
    public void TestDefaultTurnResult()
    {
        TurnResult turnResult = new TurnResult(TurnStatus.Success);
        Assert.AreEqual(turnResult.Status, TurnStatus.Success);
        Assert.AreEqual(turnResult.WinnerSeed, Seed.None);
    }
}
