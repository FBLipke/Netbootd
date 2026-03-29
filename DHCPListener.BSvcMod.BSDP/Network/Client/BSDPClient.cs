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

namespace DHCPListener.BSvcMod.BSDP
{
	public class BSDPClient : IBSDPClient
	{

		public Architecture Architecture {get; set; }
		
		public DHCPPacket Response {get; set; }

		public DHCPPacket Request {get; set; }
		
		public DHCPVendorID VendorId {get; set; }
		
		public Guid Id {get; set; }
		
		public Guid Socket {get; set; }
		
		public Guid Server {get; set; }
		
		public Guid Client {get; set; }

        public bool TestClient { get; set; }

        public NicSpecType NicSpecType { get; set; }

		public BSDPMsgType BSDPMsgType { get; set; }

		public Version BSDPVersion { get; set; }

		private void _ctorFunc()
		{
			VendorId = Request.GetVendorIdent;
			Response = new DHCPPacket();

			var encaps = Request.GetEncOptions((byte)43).Values;
			foreach (var option in encaps)
			{
				switch ((BSDPVendorEncOptions)option.Option)
				{
					case BSDPVendorEncOptions.MessageType:
						BSDPMsgType = (BSDPMsgType)option.AsByte();
						break;
					case BSDPVendorEncOptions.Version:
						BSDPVersion = new Version(option.Data.First(), option.Data.Last());
						break;
					case BSDPVendorEncOptions.ServerIdentifier:

						break;
					case BSDPVendorEncOptions.ServerPriority:
						break;
					case BSDPVendorEncOptions.ReplyPort:
						break;
					case BSDPVendorEncOptions.BootImageListPath:
						break;
					case BSDPVendorEncOptions.DefaultBootImage:
						break;
					case BSDPVendorEncOptions.SelectedBootImage:
						break;
					case BSDPVendorEncOptions.BootImageList:
						break;
					case BSDPVendorEncOptions.Netboot10Firmware:
						break;
					case BSDPVendorEncOptions.AttributesFilterList:
						break;
					case BSDPVendorEncOptions.MaxMessageSize:
						break;
					default:
						break;
				}
			}






		}

		public BSDPClient(bool testClient, Guid id, DHCPPacket request, Guid server, Guid socket, Guid client)
		{
			Server = server;
			Client = client;
			Socket = socket;

			TestClient = testClient;

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
