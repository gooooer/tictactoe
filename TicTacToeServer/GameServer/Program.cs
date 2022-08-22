using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Networker.Extensions.Json;
using Networker.Server;
using Networker.Server.Abstractions;
using TicTacToeLib.Protocol;
using Utf8Json;
using Utf8Json.Resolvers;

namespace GameServer;

public class Program
{
    public static void Main()
    {
        JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivateExcludeNullSnakeCase);

        IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();

        IConfigurationSection networkerSettings = config.GetSection("Networker");

        IServer server = new ServerBuilder()
            .UseTcp(networkerSettings.GetValue<int>("TcpPort"))
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(config.GetSection("Logging"));
                loggingBuilder.AddConsole();
            })
            .UseJson()
            .RegisterPacketHandler<CommandPacket, CommandPacketHandler>()
            .Build();

        SingleRoomManager.Instance.Server = server;
        server.ClientConnected += SingleRoomManager.Instance.ClientConnected;
        server.ClientDisconnected += SingleRoomManager.Instance.ClientDisconnected;
        server.Start();
        
        Process.GetCurrentProcess().WaitForExit();
    }
}
