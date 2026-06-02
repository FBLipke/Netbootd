namespace Netboot.Module.DHCPListener
{
    public interface IBootService
    {
        BootServerType ServerType { get; set; }

        void Handle_BootService_Request(Guid client, DHCPPacket requestPacket);

        void Handle_BootService_Request(string client, DHCPPacket requestPacket);

        void HeartBeat();

        void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream);
    }
}
