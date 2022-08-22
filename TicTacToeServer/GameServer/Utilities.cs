using System.Net;
using System.Net.Sockets;

namespace GameServer;

public static class Utilities
{
    public static string GetUserOrigin(Socket socket)
    {
        IPEndPoint? remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;

        if (remoteEndPoint != null)
        {
            return Utilities.GetUserOrigin(remoteEndPoint);
        }
        else
        {
            return String.Empty;
        }
    }

    public static string GetUserOrigin(IPEndPoint endPoint)
    {
        return endPoint.ToString();
    }
}
