using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Services.DHCP
{
    public class BOOTP
    {
        public BOOTP() { }
    }

    public class DHCP : BOOTP
    {
        public DHCP() : base () {
        }

        public void Handle_DHCP_Discover(Guid serverId, Guid socketId, IPacket packet, IClient client)
        {
            var response = new DHCPPacket(packet.ServerType, new byte[1024]);
            NetbootBase.Servers[serverId].Send(socketId, response, client);
        }

        public void Handle_DHCP_Offer(IPacket packet) { }

        public void Handle_DHCP_Request(IPacket packet) { }

        public void Handle_DHCP_ACK(IPacket packet) { }

        public void Handle_DHCP_NAK(IPacket packet) { }

        public void Handle_DHCP_Release(IPacket packet) { }

        public void Handle_DHCP_Decline(IPacket packet) { }
    }
}
