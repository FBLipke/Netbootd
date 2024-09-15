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
using Netboot.Network.Sockets;
using System.Xml;

namespace Netboot.Services.Interfaces
{
	public interface IService : IDisposable
	{
        delegate void PrintMessageEventHandler(object sender, PrintMessageEventArgs e);
        delegate void AddServerEventHandler(object sender, AddServerEventArgs e);
		delegate void ServerSendPacketEventHandler(object sender, ServerSendPacketEventArgs e);
        event PrintMessageEventHandler? PrintMessage;
        event AddServerEventHandler? AddServer;
		event ServerSendPacketEventHandler? ServerSendPacket;

		List<ushort> Ports { get; set; }

		string ServiceType { get; }

		SocketProtocol Protocol { get; set; }

		void Handle_DataReceived(object sender, DataReceivedEventArgs e);
		void Handle_DataSent(object sender, DataSentEventArgs e);

		void Heartbeat();
		void Start();
		void Stop();

		bool Initialize(XmlNode xmlConfigNode);
	}
}
