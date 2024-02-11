using Netboot.Network.Interfaces;

namespace Netboot.Network.Packet
{
    public class BasePacket : IPacket
    {
        public MemoryStream Buffer { get; set; }

        public string ServiceType { get; set; }

        public BasePacket(string serviceType, byte[] data)
        {
            Buffer = new MemoryStream(data);
            ServiceType = serviceType;
        }

        public void Dispose()
        {
        }
    }
}
