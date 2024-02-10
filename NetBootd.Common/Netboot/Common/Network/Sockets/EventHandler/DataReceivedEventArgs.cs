using System.Net;

namespace Netboot.Common.Network.Sockets
{
    public class DataReceivedEventArgs
    {
        public byte[] Data { get; private set; }
        public IPEndPoint RemoteEndpoint { get; private set; }
		public Guid SocketId { get; private set; }

		public DataReceivedEventArgs(Guid socketId, byte[] buffer, IPEndPoint remoteEndpoint)
        {
            SocketId = socketId;
            Data = buffer;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}