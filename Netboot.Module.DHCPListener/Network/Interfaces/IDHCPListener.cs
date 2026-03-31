namespace Netboot.Module.DHCPListener.Interfaces
{
    public interface IDHCPListener
    {
        void Handle_DHCP_Discover(Guid clientid, DHCPPacket request);

        void Handle_DHCP_Request(Guid clientid, DHCPPacket request);
    }
}
