using System.Net;

namespace Netboot.Common.Network.Sockets
{
    public class DataSendEventArgs
    {
        public IPEndPoint RemoteEndpoint { get; private set; }
        public int BytesSent { get; private set; }
        public Guid SocketId { get; private set; }

        public DataSendEventArgs(Guid socketId, int bytessent, IPEndPoint remoteEndpoint)
        {
            SocketId = socketId;
            BytesSent = bytessent;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}