using Microsoft.Extensions.Logging;
using Networker.Extensions.Json;
using Networker.Server;
using Networker.Server.Abstractions;
using TicTacToeLib.Game;
using TicTacToeLib.Protocol;

namespace GameServer;

// This is a simple and loose version of game room manager that only handles one game at a time.
// Each user gets assigned a unique ID upon joining the game. Those IDs are later
// used by the game library to identify every user's turn. This class lets more than
// two users join. In this case, users beyond second are becoming spectators.
// TODO:
// An appropriate Room class has to be introduced for multiroom support, and
// part of the private fields should be transferred there.
public class SingleRoomManager
{
    // Singleton code
    private static readonly SingleRoomManager _instance = new SingleRoomManager();

    static SingleRoomManager()
    {
    }

    private SingleRoomManager()
    {
        _game = new Game();
        _connectedUsers = new Dictionary<string, User>();
        _lastAssignedUserId = Game.UnknownUserId;

        _logger = LoggerFactory.Create(config =>
        {
            config.SetMinimumLevel(LogLevel.Debug);
            config.AddConsole();
        }).CreateLogger("SingleRoomManager");
        _jsonSerialiser = new JsonSerialiser();
    }

    public static SingleRoomManager Instance
    {
        get
        {
            return _instance;
        }
    }

    // Utilities
    private ILogger _logger;
    private JsonSerialiser _jsonSerialiser;

    // Room management
    public IServer? Server { get; set; }

    private Dictionary<string, User> _connectedUsers;
    private int _lastAssignedUserId;

    private Game _game;

    // Methods

    // Callback from server upon connection receipt.
    // Every user connection is represented by its origin which is IP address and remote port.
    public void ClientConnected(object? sender, TcpConnectionConnectedEventArgs e)
    {
        string userOrigin = Utilities.GetUserOrigin(e.Connection.Socket);
        _logger.LogInformation("User has connected but hasn't joined the game. Origin: " + userOrigin);

        _connectedUsers.Add(userOrigin, new User());
    }

    // Callback from the server upon connection drop. Remove user from the map.
    public void ClientDisconnected(object? sender, TcpConnectionDisconnectedEventArgs e)
    {
        string userOrigin = Utilities.GetUserOrigin(e.Connection.Socket);
        User user = _connectedUsers[userOrigin];

        _connectedUsers.Remove(userOrigin);

        _logger.LogInformation("User has disconnected. Origin: " + userOrigin + " PlayerID: " + user.AssignedPlayerId);
    }

    // When a client sends the packet to join the game, this method assigns him a unique ID and sends it back.
    // Later this ID could be used by the game to identify who is making a turn, who is the winner, etc.
    public int JoinGame(string userOrigin)
    {
        _logger.LogInformation("User with origin: " + userOrigin + " wants to join the game");
        // Check if joining user is in the "lobby" list
        if (_connectedUsers.ContainsKey(userOrigin))
        {
            _lastAssignedUserId += 1;
            _connectedUsers[userOrigin].AssignedPlayerId = _lastAssignedUserId;

            // The game only begins when the second user makes a request to join the game.
            // All following users would remain spectators.
            if (_connectedUsers.Count == 2)
            {
                ScheduleNewGame();
            }
            else if (_connectedUsers.Count > 2)
            {
                SendToUserOrigin(CommandPacket.FromGameState(_game), userOrigin);
            }

            return _connectedUsers[userOrigin].AssignedPlayerId;
        }
        else
        {
            return Game.UnknownUserId;
        }
    }

    public TurnResult MakeTurn(Turn turn)
    {
        // Advance to next local server game state
        TurnResult turnResult = _game.MakeTurn(turn);

        if (turnResult.Status == TurnStatus.Success)
        {
            ScheduleBroadcastNewTurn(turn);
        }

        return turnResult;
    }

    // This function initializes the game's state on the server side and broadcasts
    // it to every connected client. Game state broadcasting can also be used later
    // for returning users or correcting errors.
    private void ScheduleNewGame()
    {
        _logger.LogInformation("Starting new game...");
        Task.Run(() =>
        {
            _game.Start(_connectedUsers.Values.ElementAt(0).AssignedPlayerId, _connectedUsers.Values.ElementAt(1).AssignedPlayerId);
            SendToAll(CommandPacket.FromGameState(_game));
        });
    }

    // This function broadcasts a new game turn to every connected client.
    // Hence everyone can advance their game state to the next level.
    private void ScheduleBroadcastNewTurn(Turn turn)
    {
        Task.Run(() =>
        {
            SendToAll(CommandPacket.FromTurn(turn));
        });
    }

    private void SendToAll<T>(T packet)
    {
        SendToRecipients(packet, Server.GetConnections().GetConnections());
    }

    private void SendToUserOrigin<T>(T packet, string userOrigin)
    {
        ITcpConnection conn = Server.GetConnections().GetConnections().Find(conn => Utilities.GetUserOrigin(conn.Socket) == userOrigin);
        if (conn != null) conn.Socket.Send(_jsonSerialiser.Serialise(packet));
    }

    private void SendToRecipients<T>(T packet, List<ITcpConnection> recipients)
    {
        foreach (ITcpConnection conn in recipients)
        {
            conn.Socket.Send(_jsonSerialiser.Serialise(packet));
        }
    }    
}
