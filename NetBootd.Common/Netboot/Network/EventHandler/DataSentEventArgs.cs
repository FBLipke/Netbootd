using System.Net;

namespace Netboot.Network.EventHandler
{
    public class DataSentEventArgs
    {
        public IPEndPoint RemoteEndpoint { get; private set; }
        public int BytesSent { get; private set; }
        public Guid SocketId { get; private set; }
        public Guid ServerId { get; private set; }
        public string ServiceType { get; }

        public DataSentEventArgs(string serviceType, Guid serverId, Guid socketId,
            int bytessent, IPEndPoint remoteEndpoint)
        {
            ServiceType = serviceType;
            ServerId = serverId;
            SocketId = socketId;
            BytesSent = bytessent;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}