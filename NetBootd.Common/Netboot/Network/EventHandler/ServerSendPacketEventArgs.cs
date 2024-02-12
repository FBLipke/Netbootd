using Netboot.Network.Interfaces;

namespace Netboot.Common.Netboot.Network.EventHandler
{
    public class ServerSendPacketEventArgs
    {
        public Guid ServerId { get; set; }
        public Guid SocketId { get; set; }
        public IPacket Packet { get; set; }
        public IClient Client { get; set; }

        public ServerSendPacketEventArgs(Guid server, Guid socket, IPacket packet, IClient client)
        {
            ServerId = server;
            SocketId = socket;
            Packet = packet;
            Client = client;
        }
    }
}