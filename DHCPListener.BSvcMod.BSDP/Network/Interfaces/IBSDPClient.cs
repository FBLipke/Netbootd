using Netboot.Module.DHCPListener;

namespace DHCPListener.BSvcMod.BSDP
{
    public interface IBSDPClient : IDHCPClient
    {
        bool TestClient { get; set; }
    }
}
