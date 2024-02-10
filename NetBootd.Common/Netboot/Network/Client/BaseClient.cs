using Netboot.Network.Definitions;
using Netboot.Network.Interfaces;
using System.Net;

namespace Netboot.Network.Client
{
    public class BaseClient : IClient
    {
        public Guid SocketId;
        public Guid ServerId;
        public ServerType ServerType;
        public Guid ClientId = Guid.Empty;
        public IPEndPoint RemoteEntpoint { get; set; }

        public BaseClient(Guid clientId, ServerType serverType, Guid serverid, Guid socketId)
        {
            ServerType = serverType;
            SocketId = socketId;
            ServerId = serverid;
            ClientId = clientId;
        }

        

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
