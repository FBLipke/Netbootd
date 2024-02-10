using Netboot.Network.Definitions;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using Netboot.Services.Interfaces;

namespace Netboot.Service.DHCP
{
    public class DHCPService : IService
    {
        public DHCPService() {
        
        }

        public void Handle_DHCP_Discover(ServerType serverType, Guid serverid, Guid socketid, DHCPPacket packet)
        {
            Console.WriteLine($"[{serverType}] Discover... \nServerId: {serverid}\nSocketId: {socketid}");
        }

        public void Handle_DHCP_Request(ServerType serverType, Guid serverid, Guid socketid, DHCPPacket packet)
        {
            Console.WriteLine($"[{serverType}] Discover... \nServerId: {serverid}\nSocketId: {socketid}");
        }
    }
}
