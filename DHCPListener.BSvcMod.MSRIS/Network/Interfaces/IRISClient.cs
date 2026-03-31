using Netboot.Module.DHCPListener;


namespace DHCPListener.BSvcMod.MSRIS
{
    public interface IRISClient : IDHCPClient, IDisposable
    {
        public Guid Id { get; set; }
    }
}
