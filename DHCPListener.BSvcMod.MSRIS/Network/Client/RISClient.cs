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

namespace DHCPListener.BSvcMod.MSRIS
{
	public class RISClient : IRISClient
	{

		public Guid Id { get; set; }
		
		public DHCPPacket? Response { get; set; }
		
		public DHCPPacket? Request { get; set; }
		
		public DHCPVendorID VendorId { get; set; }
		
		public NicSpecType NicSpecType { get; set; }
		
		public Guid Socket { get; set; }
		
		public Guid Server { get; set; }
		
		public Guid Client { get; set; }
        public Architecture Architecture { get; set; } = Architecture.X86PC;

        private void _ctorFunc()
		{
			VendorId = Request.GetVendorIdent;
			Response = new DHCPPacket();
		}

		public RISClient(Guid id, DHCPPacket request, Guid server, Guid socket, Guid client)
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
