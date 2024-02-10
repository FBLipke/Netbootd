using Netboot.Network.Definitions;
using System.Net;

namespace Netboot.Network.EventHandler
{
    public class DataSendEventArgs
    {
        public IPEndPoint RemoteEndpoint { get; private set; }
        public int BytesSent { get; private set; }
        public Guid SocketId { get; private set; }
        public Guid ServerId { get; private set; }
        public ServerType ServerType { get; private set; }

        public DataSendEventArgs(ServerType serverType, Guid serverId, Guid socketId,
            int bytessent, IPEndPoint remoteEndpoint)
        {
            ServerType = serverType;
            ServerId = serverId;
            SocketId = socketId;
            BytesSent = bytessent;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}