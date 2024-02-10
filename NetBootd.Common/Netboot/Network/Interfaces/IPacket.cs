using Netboot.Network.Definitions;

namespace Netboot.Network.Interfaces
{
    public interface IPacket : IDisposable
    {
        byte[] Data { get; set; }

        ServerType ServerType { get; set; }
    }
}
