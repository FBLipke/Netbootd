using Netboot.Common.Netboot.Network.EventHandler;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using System.Xml;

namespace Netboot.Services.Interfaces
{
    public interface IService : IDisposable
    {
        delegate void AddServerEventHandler(object sender, AddServerEventArgs e);
		delegate void ServerSendPacketEventHandler(object sender, ServerSendPacketEventArgs e);
		event AddServerEventHandler? AddServer;
		event ServerSendPacketEventHandler? ServerSendPacket;

		List<ushort> Ports { get; set; }

        string ServiceType { get; }

        void Handle_DataReceived(object sender, DataReceivedEventArgs e);
        void Handle_DataSent(object sender, DataSentEventArgs e);

		void Heartbeat();
		void Start();
        void Stop();

        bool Initialize(XmlNode xmlConfigNode);
    }
}
