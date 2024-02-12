using Netboot.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Network.Client
{
	internal class DHCPClient : BaseClient
	{
		public DHCPClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
			: base(clientId, serviceType, remoteEndpoint, serverid, socketId)
		{
		}
	}
}
