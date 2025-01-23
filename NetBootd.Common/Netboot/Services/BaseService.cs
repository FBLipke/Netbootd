/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Sockets;
using Netboot.Services.Interfaces;
using System.Xml;

namespace Netboot.Services
{
	public class BaseService : IService
	{
		public BaseService(string serviceType, SocketProtocol protocol)
		{
			ServiceType = serviceType;
			Protocol = protocol;
		}

		public Dictionary<string, IClient> Clients { get; set; } = [];

		public List<ushort> Ports { get; set; } = [];

		public string ServiceType { get; }
		public SocketProtocol Protocol { get; set; }

		public event IService.AddServerEventHandler? AddServer;
		public event IService.ServerSendPacketEventHandler? ServerSendPacket;
		public event IService.PrintMessageEventHandler? PrintMessage;

		public void Dispose()
		{
			foreach (var client in Clients.Values.ToList())
				client.Dispose();

			Clients.Clear();
			Ports.Clear();
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
		{
		}

		public void Heartbeat(DateTime now)
		{
		}

		public bool Initialize(XmlNode xmlConfigNode)
		{
			AddServer.Invoke(this, new(ServiceType, Protocol, Ports));
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
