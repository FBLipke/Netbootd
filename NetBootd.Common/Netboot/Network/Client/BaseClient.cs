using Netboot.Network.Interfaces;
using System.Net;

namespace Netboot.Network.Client
{
    public class BaseClient : IClient
    {
        public Guid SocketId { get; set; }
        public Guid ServerId { get; set; }
        public string ServiceType { get; set; }
        public string ClientId { get; set; }

        public IPEndPoint RemoteEntpoint { get; set; }

        public BaseClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
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
