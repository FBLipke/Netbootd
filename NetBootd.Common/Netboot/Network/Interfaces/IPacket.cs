namespace Netboot.Network.Interfaces
{
    public interface IPacket : IDisposable
    {
        MemoryStream Buffer { get; set; }

        string ServiceType { get; }
    }
}
