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
		public DHCPClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId, PXEVendorID vendorID)
			: base(clientId, serviceType, remoteEndpoint, serverid, socketId)
		{
			RBCP = new RBCPClient();
			WDS = new WDSClient();
			Response = new DHCPPacket();
		}

		public Architecture Architecture { get; set; } = Architecture.X86PC;

		public RBCPClient RBCP { get; private set; }

		public WDSClient WDS { get; private set; }

		public DHCPPacket Response { get; set; }

		public override void Dispose()
		{
			Response.Dispose();
			base.Dispose();
		}
	}
}
