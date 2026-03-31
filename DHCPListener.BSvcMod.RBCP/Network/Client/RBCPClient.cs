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

namespace DHCPListener.BSvcMod.RBCP
{
    public class RBCPClient : IRBCPClient
    {
        public ushort Layer { get; set; }

        public ushort Item { get; set; }

        public Architecture Architecture { get; set; }

        public DHCPPacket Response { get; set; }

        public DHCPPacket Request { get; set; }

        public DHCPVendorID VendorId { get; set; }

        public NicSpecType NicSpecType { get; set; }

        public Guid Id { get; set; }

        public Guid Socket { get; set; }

        public Guid Server { get; set; }

        public Guid Client { get; set; }

        public bool TestClient { get; set; }

        private void _ctorFunc()
        {
            VendorId = Request.GetVendorIdent;
            Response = new DHCPPacket();
            Layer = 0;
            Item = 0;

            Architecture = (Netboot.Module.DHCPListener.Architecture)
                Request.GetOption((byte)DHCPOptions.SystemArchitectureType).AsUInt16();

            NicSpecType = NicSpecType.UNDI;
        }

        public RBCPClient(bool testClient, Guid id, DHCPPacket request, Guid server, Guid socket, Guid client)
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
