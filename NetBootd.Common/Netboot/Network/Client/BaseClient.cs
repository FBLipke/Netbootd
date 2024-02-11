using Netboot.Network.Interfaces;
using System.Net;

namespace Netboot.Network.Client
{
    public class BaseClient : IClient
    {
        public Guid SocketId;
        public Guid ServerId;
        public string ServiceType;
        public Guid ClientId = Guid.Empty;
        public IPEndPoint RemoteEntpoint { get; set; }

        public BaseClient(Guid clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
        {
            ServiceType = serviceType;
            SocketId = socketId;
            ServerId = serverid;
            ClientId = clientId;
            RemoteEntpoint = remoteEndpoint;
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
