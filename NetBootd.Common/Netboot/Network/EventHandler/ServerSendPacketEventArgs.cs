using Netboot.Network.Interfaces;

namespace Netboot.Common.Netboot.Network.EventHandler
{
    public class ServerSendPacketEventArgs
    {
        public Guid ServerId { get; set; }
        public Guid SocketId { get; set; }

		public string ServiceType { get; set; }

		public IPacket Packet { get; set; }
        public IClient Client { get; set; }

        public ServerSendPacketEventArgs(string serviceType, Guid server, Guid socket, IPacket packet, IClient client)
        {
            ServiceType = serviceType;
            ServerId = server;
            SocketId = socket;
            Packet = packet;
            Client = client;
        }
    }
}