using TicTacToeLib.Game;
using Utf8Json;

namespace TicTacToeLib.Protocol;

public enum ResultType
{
    StatusMessage,
    JoinResult,
    TurnResult
}

public class ResultPacket
{
    public ResultType Type;
    public string JsonData;
    
    public static ResultPacket FromStatusCodeAndMessage(int code, string message)
    {
        StatusMessage statusMessage = new StatusMessage()
        {
            statusCode = code,
            message = message
        };

        return new ResultPacket { Type = ResultType.StatusMessage, JsonData = JsonSerializer.ToJsonString(statusMessage) };
    }

    public static ResultPacket FromJoinResult(int assignedPlayerId)
    {
        return new ResultPacket { Type = ResultType.JoinResult, JsonData = JsonSerializer.ToJsonString(assignedPlayerId) };
    }

    public static ResultPacket FromTurnResult(TurnResult turnResult)
    {
        return new ResultPacket { Type = ResultType.TurnResult, JsonData = JsonSerializer.ToJsonString(turnResult) };
    }
}

public struct StatusMessage
{
    public string message;
    public int statusCode;
}

public struct JoinResult
{
    public int assignedPlayerId;
}
