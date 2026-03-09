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

using Netboot.Module.DHCPListener;
using System.Net;

namespace DHCPListener.BSvcMod.RBCP
{
	public class RBCPClient : IRBCPClient
	{
		public ushort Layer { get; set; }

		public ushort Item { get; set; }

		public IPAddress McastDiscoveryAddress {get; set; }
		
		public ushort McastClientPort {get; set; }
		
		public ushort McastServerPort {get; set; }
		
		public byte DiscoveryControl {get; set; }
		
		public byte MulticastTimeout {get; set; }
		
		public byte MulticastDelay {get; set; }
		
		public Architecture Architecture {get; set; }
		
		public DHCPPacket Response {get; set; }

		public DHCPPacket Request {get; set; }
		
		public DHCPVendorID VendorId {get; set; }
		
		public NicSpecType NicSpecType {get; set; }
		
		public Guid Id {get; set; }
		
		public Guid Socket {get; set; }
		
		public Guid Server {get; set; }
		
		public Guid Client {get; set; }

		private void _ctorFunc()
		{
			VendorId = Request.GetVendorIdent;
			Response = new DHCPPacket();
			Layer = 0;
			Item = 0;

			DiscoveryControl = 0;
			McastDiscoveryAddress = IPAddress.Parse("224.0.1.2");
			McastServerPort = 69;
			McastClientPort = 4001;
			MulticastDelay = 4;
			MulticastTimeout = 4;
			Architecture = (Netboot.Module.DHCPListener.Architecture)
				Request.GetOption((byte)DHCPOptions.SystemArchitectureType).AsUInt16();

			NicSpecType = NicSpecType.UNDI;
		}

		public RBCPClient(Guid id, DHCPPacket request, Guid server, Guid socket, Guid client)
		{
			Server = server;
			Client = client;
			Socket = socket;

			Request = request;
			Id = id;
			_ctorFunc();
		}

		public void Dispose()
		{
			Request.Dispose();
			Response.Dispose();
		}
	}
}
