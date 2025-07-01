/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Netboot.Network.Definitions;
using Netboot.Network.Packet;
using System.Net;

namespace Netboot.Network.Client
{
	public partial class DHCPClient : BaseClient
	{
		public DHCPClient(bool testClient, string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId, DHCPVendorID vendorID)
			: base(testClient, clientId, serviceType, remoteEndpoint, serverid, socketId)
		{
			RBCP = new RBCPClient();
			WDS = new WDSClient();
			BSDP = new BSDPClient();
			VendorID = vendorID;
			Response = new DHCPPacket();
			NetBootdClient = testClient;
		}

		public bool NetBootdClient { get; private set; }

		public Architecture Architecture { get; set; }

		public RBCPClient RBCP { get; private set; }

		public WDSClient WDS { get; private set; }

		public BSDPClient BSDP { get; private set; }

		public DHCPPacket Response { get; set; }

		public DHCPVendorID VendorID { get; private set; }

		public override void Dispose()
		{
			Response.Dispose();
			base.Dispose();
		}

		public override void Heartbeat()
		{
			var packet = new DHCPPacket();
			base.Heartbeat();
		}
	}
}
