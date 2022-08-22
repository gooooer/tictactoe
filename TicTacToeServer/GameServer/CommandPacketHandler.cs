using Microsoft.Extensions.Logging;
using Networker.Common;
using Networker.Common.Abstractions;
using TicTacToeLib.Game;
using TicTacToeLib.Protocol;
using Utf8Json;

namespace GameServer;

interface ICommandAction
{
    public void perform(string jsonData, IPacketContext packetContex);
}

class JoinAction : ICommandAction
{
    public void perform(string jsonData, IPacketContext packetContext)
    {
        // There is no authentication here. Just assign user IDs as people join.
        int assignedUserId = SingleRoomManager.Instance.JoinGame(Utilities.GetUserOrigin(packetContext.Sender.EndPoint));
        packetContext.Sender.Send(ResultPacket.FromJoinResult(assignedUserId));
    }
}

class MakeTurnAction : ICommandAction
{
    public void perform(string jsonData, IPacketContext packetContext)
    {        
        Turn? turn = JsonSerializer.Deserialize<Turn>(jsonData);
        if (turn != null)
        {
            TurnResult turnResult = SingleRoomManager.Instance.MakeTurn(turn.Value);
            packetContext.Sender.Send(ResultPacket.FromTurnResult(turnResult));
        }
        else
        {
            packetContext.Sender.Send(ResultPacket.FromStatusCodeAndMessage(-1, "Unrecognized turn data"));
        }
    }
}

// Packet handler to handle commands coming from clients.
public class CommandPacketHandler : PacketHandlerBase<CommandPacket>
{
    private ILogger<CommandPacketHandler> _logger;
    private Dictionary<Command, ICommandAction> _actions;

    public CommandPacketHandler(ILogger<CommandPacketHandler> logger)
    {
        _logger = logger;
        _actions = new Dictionary<Command, ICommandAction>()
        {
            { Command.JoinGame, new JoinAction() },
            { Command.MakeTurn, new MakeTurnAction() }
        };
    }

    public override Task Process(CommandPacket packet, IPacketContext packetContext)
    {
        _logger.LogDebug("Server received CommandPacket from: " + Utilities.GetUserOrigin(packetContext.Sender.EndPoint));

        Task t = new Task(() =>
        {
            if (_actions.ContainsKey(packet.Command))
            {
                _actions[packet.Command].perform(packet.JsonData, packetContext);
            }
            else
            {
                _logger.LogError("Unrecognized command: " + packet.Command);
                packetContext.Sender.Send(ResultPacket.FromStatusCodeAndMessage(-1, "Unrecognized command"));
            }
        });
        t.Start();

        return t;
    }
}
