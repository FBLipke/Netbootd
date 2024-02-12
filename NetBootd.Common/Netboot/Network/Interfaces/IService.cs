using Netboot.Common.Netboot.Network.EventHandler;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;

namespace Netboot.Services.Interfaces
{
    public interface IService : IDisposable
    {
        delegate void AddServerEventHandler(object sender, AddServerEventArgs e);
		delegate void ServerSendPacketEventHandler(object sender, ServerSendPacketEventArgs e);
		event AddServerEventHandler? AddServer;
		event ServerSendPacketEventHandler? ServerSendPacket;

		List<ushort> Ports { get; }

        string ServiceType { get; }

        Dictionary<string, IClient> Clients { get; set; }

        void Handle_DataReceived(object sender, DataReceivedEventArgs e);
        void Handle_DataSent(object sender, DataSentEventArgs e);

        void Start();
        void Stop();

        bool Initialize();
    }
}
