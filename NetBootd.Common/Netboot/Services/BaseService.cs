using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;
using System.Xml;

namespace Netboot.Services
{
	public class BaseService : IService
	{
		public BaseService(string serviceType)
		{
			ServiceType = serviceType;
		}

		public Dictionary<string, IClient> Clients { get; set; } = [];

		public List<ushort> Ports { get; } = [];

		public string ServiceType { get; }

		public event IService.AddServerEventHandler? AddServer;
		public event IService.ServerSendPacketEventHandler? ServerSendPacket;

		public void Dispose()
		{
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void Heartbeat()
		{
		}

		public bool Initialize(XmlNode xmlConfigNode)
		{
			AddServer.Invoke(this, new(ServiceType, Ports));
			return true;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}
	}
}
