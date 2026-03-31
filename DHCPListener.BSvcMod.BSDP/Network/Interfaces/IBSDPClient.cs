using Netboot.Module.DHCPListener;

namespace DHCPListener.BSvcMod.BSDP
{
    public interface IBSDPClient : IDHCPClient
    {
        public BSDPMsgType BSDPMsgType { get; set; }

        public Version BSDPVersion { get; set; }
    }
}
