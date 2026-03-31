using Netboot.Module.DHCPListener;

namespace DHCPListener.BSvcMod.RBCP
{
    public interface IRBCPClient : IDHCPClient
    {
        public ushort Layer { get; set; }

        public ushort Item { get; set; }

        bool TestClient { get; set; }
    }
}
