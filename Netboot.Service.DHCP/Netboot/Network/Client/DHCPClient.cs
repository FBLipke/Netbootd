using System.Net;

namespace Netboot.Network.Client
{
	public class DHCPClient : BaseClient
	{
		public class RBCPClient
		{
			public ushort Layer { get; set; }

			public ushort Item { get; set; }

			public RBCPClient()
			{
				Layer = 0;
				Item = 0;
			}
		}

		public DHCPClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
			: base(clientId, serviceType, remoteEndpoint, serverid, socketId)
		{
			RBCP = new RBCPClient();
		}

		public RBCPClient RBCP { get; private set; }
	}
}
