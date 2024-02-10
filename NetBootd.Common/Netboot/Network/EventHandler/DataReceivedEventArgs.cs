using Netboot.Network.Interfaces;
using Netboot.Network.Definitions;
using System.Net;

namespace Netboot.Network.EventHandler
{
    public class DataReceivedEventArgs
    {
        public IPacket Packet { get; private set; }
        public IPEndPoint RemoteEndpoint { get; private set; }
        public Guid SocketId { get; private set; }
        public Guid ServerId { get; private set; }
        public ServerType ServerType { get; private set; }

        public DataReceivedEventArgs(ServerType serverType, Guid serverId,
            Guid socketId, IPacket packet, IPEndPoint remoteEndpoint)
        {
            ServerType = serverType;
            ServerId = serverId;
            SocketId = socketId;
            Packet = packet;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}