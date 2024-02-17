using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;
using System.Xml;

namespace Netboot.Service.BINL
{
	public class BINLService : IService
	{
		public BINLService(string serviceType)
		{
			ServiceType = serviceType;
		}

		public List<ushort> Ports { get; set; } = [];

		public string ServiceType { get; }

		public Dictionary<string, IClient> Clients { get; set; } = [];

		public event IService.AddServerEventHandler? AddServer;
		public event IService.ServerSendPacketEventHandler? ServerSendPacket;

		public void Dispose()
		{
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
		{
		}

		public void Heartbeat()
		{
		}

		public bool Initialize(XmlNode xmlConfigNode)
		{
			return false;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}
	}
}
