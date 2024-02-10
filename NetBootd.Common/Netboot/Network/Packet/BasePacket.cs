using Netboot.Network.Definitions;
using Netboot.Network.Interfaces;

namespace Netboot.Network.Packet
{
    public class BasePacket : IPacket
    {
        public byte[] Data { get; set; }

        public ServerType ServerType { get; set; }

        public BasePacket(ServerType serverType, byte[] data)
        {
            Data = data;
            ServerType = serverType;
        }

        public void Dispose()
        {
        }
    }
}
