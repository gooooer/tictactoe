using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Networker.Common;
using Networker.Common.Abstractions;
using TicTacToeLib.Protocol;
using Utf8Json;
using UnityEngine;

public class ResultPackethandler : PacketHandlerBase<ResultPacket>
{
    private ILogger<CommandPackethandler> _logger;
    private IBoardRemoteControl _board;

    public ResultPackethandler(ILogger<CommandPackethandler> logger)
    {
        _logger = logger;
        _board = GameObject.Find("Board").GetComponent<Board>();
    }

    public override Task Process(ResultPacket packet, IPacketContext packetContext)
    {
        _logger.LogDebug("Received result from server: " + packet.JsonData);

        Task t = new Task(() =>
        {
            if (packet.Type == ResultType.JoinResult)
            {
                _board.AssignPlayerId(JsonSerializer.Deserialize<int>(packet.JsonData));
            }
        });

        t.Start();

        return t;
    }
}
