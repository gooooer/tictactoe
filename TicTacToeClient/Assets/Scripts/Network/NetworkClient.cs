using Microsoft.Extensions.Logging;
using Networker.Client;
using Networker.Client.Abstractions;
using Networker.Extensions.Json;
using TicTacToeLib.Protocol;
using TicTacToeLib.Game;
using UnityEngine;
using Utf8Json;
using Utf8Json.Resolvers;

// This class translates game-level entities into network packets.
public class NetworkClient : MonoBehaviour, IGameSession
{
    private IClient _client;

    // Unity component lifecycle

    void Start()
    {
        JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivateExcludeNullSnakeCase);

        // TODO: take init params from config
        _client = new ClientBuilder()
            .UseIp("localhost")
            .UseTcp(18200)
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
            })
            .RegisterPacketHandler<ResultPacket, ResultPackethandler>()
            .RegisterPacketHandler<CommandPacket, CommandPackethandler>()
            .UseJson()
            .Build();

        ConnectResult result = _client.Connect();

        if (result.Success)
        {
            _client.Send(CommandPacket.FromJoinCommand());
        }
        else
        {
            Debug.Log("Cannot connect to the server!");
        }
    }

    void OnDestroy()
    {
        _client.Stop();
    }

    // IGameSession

    public void MakeTurn(Turn turn)
    {
        _client.Send(CommandPacket.FromTurn(turn));
    }
}
