using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;

namespace Netboot.Service.DHCP
{
	public class DHCPService : IService
	{
		public DHCPService(string serviceType) {
			ServiceType = serviceType;
		}

		public List<ushort> Ports => new List<ushort> { 67, 4011 };

		public string ServiceType { get; }

		public Dictionary<Guid, IClient> Clients { get; set; } = [];

		public event IService.AddServerEventHandler? AddServer;

		public void Dispose()
		{
			Ports.Clear();
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine("Service: DHCP!");
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
		{
		}

		public bool Initialize()
		{
			AddServer?.Invoke(this, new AddServerEventArgs(ServiceType,Ports));
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
