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

using Netboot.Common.Network.Interfaces;
using Netboot.Module.DHCPListener;
using System.Net;
using System.Net.Sockets;

namespace DHCPListener.BSvcMod.MSWDS
{
	public class WDSClient : IWDSClient
	{
		public ushort PollInterval { get; set; } = 4;

		public ushort RetryCount { get; set; } = 10;

		public PXEPromptOptionValues PXEPromptDone { get; set; }

		public PXEPromptOptionValues PXEPromptAction { get; set; } = PXEPromptOptionValues.OptIn;

		public uint ServerFeatures { get; set; }

		public bool ActionDone { get; set; } = false;

		public NextActionOptionValues NextAction { get; set; } = NextActionOptionValues.Approval;
		
		public string Message { get; set; } = string.Empty;

		public bool ServerSelection { get; set; } = false;

		public uint RequestId { get; set; } = 0;
		
		public bool VersionQuery { get; set; } = false;

		public NBPVersionValues ServerVersion { get; set; }

		public IPAddress ReferralServer { get; set; }
		
		public NBPVersionValues NBPVersion { get; set; }
		
		public Guid Id { get; set; }
		
		public Architecture Architecture {get; set; }
		
		public DHCPPacket Response {get; set; }
		
		public DHCPPacket Request {get; set; }
		
		public DHCPVendorID VendorId {get; set; }
		
		public NicSpecType NicSpecType {get; set; }
		
		public Guid Socket {get; set; }
		
		public Guid Server {get; set; }
		
		public Guid Client {get; set; }

		private void _ctorFunc()
		{
			VendorId = Request.GetVendorIdent;
			Response = new DHCPPacket();
		}

		public WDSClient(Guid id, DHCPPacket request, Guid server, Guid socket, Guid client)
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
