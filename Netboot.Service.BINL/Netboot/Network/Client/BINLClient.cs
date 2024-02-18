using System.Net;

namespace Netboot.Network.Client
{
    public class BINLClient : BaseClient
    {
        public BINLClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
            : base(clientId, serviceType, remoteEndpoint, serverid, socketId)
        {
        }


    }
}
