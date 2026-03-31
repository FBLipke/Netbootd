namespace Netboot.Module.DHCPListener
{
    public interface IDHCPClient
    {
        Architecture Architecture { get; set; }

        public DHCPPacket Response { get; set; }

        public DHCPPacket Request { get; set; }

        public DHCPVendorID VendorId { get; set; }

        public NicSpecType NicSpecType { get; set; }

        public Guid Id { get; set; }

        Guid Socket { get; set; }

        Guid Server { get; set; }

        Guid Client { get; set; }
    }
}
