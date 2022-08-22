using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Networker.Common;
using Networker.Common.Abstractions;
using Utf8Json;
using TicTacToeLib.Game;
using TicTacToeLib.Protocol;
using UnityEngine;

interface ICommandAction
{
    public void perform(string jsonData, IPacketContext packetContex, IBoardRemoteControl board);
}

class MakeTurnAction : ICommandAction
{
    public void perform(string jsonData, IPacketContext packetContext, IBoardRemoteControl board)
    {
        Turn? turn = JsonSerializer.Deserialize<Turn>(jsonData);
        if (turn != null)
        {
            board.ApplyTurnFromServer(turn.Value);
        }
    }
}

class UpdateGameStateAction : ICommandAction
{
    public void perform(string jsonData, IPacketContext packetContext, IBoardRemoteControl board)
    {
        Game? game = JsonSerializer.Deserialize<Game>(jsonData);
        if (game != null)
        {
            board.ApplyGameStateFromServer(game);
        }
    }
}

public class CommandPackethandler : PacketHandlerBase<CommandPacket>
{
    private ILogger<CommandPackethandler> _logger;
    private IBoardRemoteControl _board;

    private Dictionary<Command, ICommandAction> _actions;

    public CommandPackethandler(ILogger<CommandPackethandler> logger)
    {
        _logger = logger;
        _board = GameObject.Find("Board").GetComponent<Board>();
        _actions = new Dictionary<Command, ICommandAction>()
        {
            { Command.UpdateGameState, new UpdateGameStateAction() },
            { Command.MakeTurn, new MakeTurnAction() }
        };
    }

    public override Task Process(CommandPacket packet, IPacketContext packetContext)
    {
        _logger.LogDebug("Received new command from server: " + packet.Command);

        Task t = new Task(() =>
        {
            if (_actions.ContainsKey(packet.Command))
            {
                _actions[packet.Command].perform(packet.JsonData, packetContext, _board);
            }
            else
            {
                _logger.LogError("Unrecognized command: " + packet.Command);
            }
        });

        t.Start();

        return t;
    }
}
