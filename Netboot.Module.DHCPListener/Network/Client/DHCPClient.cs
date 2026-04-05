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

using Netboot.Common;
using Netboot.Common.Common.Definitions;
using System.Text;

namespace Netboot.Module.DHCPListener
{
    public class DHCPClient : IDHCPClient
    {
        public DHCPClient(bool testClient, Guid server, Guid socket, Guid client, DHCPPacket request)
        {
            Socket = socket;
            Client = client;
            Server = server;

            Request = request;
            VendorId = Request.GetVendorIdent;
            
            Response = Request.CreateResponse(NetbootBase.NetworkManager.ServerManager.GetEndPoint(server, socket).Address);
            Response.AddOption(new((byte)DHCPOptions.VendorClassIdentifier, VendorId.ToString(), Encoding.ASCII));

            if (request.HasOption(DHCPOptions.NetworkInterfaceIdentifier))
                NicSpecType = (NicSpecType)request.GetOption((byte)DHCPOptions.NetworkInterfaceIdentifier).AsByte();

            Architecture = (Architecture)
                Request.GetOption((byte)DHCPOptions.SystemArchitectureType).AsUInt16();

            TestClient = testClient;
        }

        private void _ctorFunc()
        {
        }

        public void Dispose()
        {
            Request?.Dispose();
            Response?.Dispose();
        }

        public bool TestClient { get; set; }

        public Architecture Architecture { get; set; }

        public DHCPPacket Response { get; set; }

        public DHCPPacket Request { get; set; }

        public DHCPVendorID VendorId { get; set; }

        public NicSpecType NicSpecType { get; set; }

        public Guid Socket { get; set; }

        public Guid Server { get; set; }

        public Guid Client { get; set; }

        public Guid Id { get; set; }
    }
}
