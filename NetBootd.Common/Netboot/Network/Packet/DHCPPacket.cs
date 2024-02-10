using Netboot.Network.Definitions;

namespace Netboot.Network.Packet
{
    public sealed class DHCPPacket : BasePacket
    {
        public DHCPMessageType MessageType { get; private set; } = DHCPMessageType.Discover;

        public DHCPPacket(ServerType serverType, byte[] data)
            : base(serverType, data)
        {
        }


    }
}
