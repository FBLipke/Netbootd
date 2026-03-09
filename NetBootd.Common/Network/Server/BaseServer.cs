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

using Netboot.Common.Common;
using Netboot.Common.Network.EventHandler;
using Netboot.Common.Network.Interfaces;
using Netboot.Common.Network.Sockets.Definition;
using System.Net;
using System.Xml;

namespace Netboot.Common.Network.Server
{
	public class BaseServer : IServer
	{
		public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
		public delegate void DataSentEventHandler(object sender, DataSentEventArgs e);
		public event DataReceivedEventHandler? DataReceived;
		public event DataSentEventHandler? DataSent;

		Dictionary<Guid, ISocket> Sockets = [];

		public Guid ServerId;
		public SocketProtocol Protocol;

		public string ServiceType { get; }

		public BaseServer(Guid serverid, string serviceType, SocketProtocol protocol, IEnumerable<ushort> ports)
		{
			ServerId = serverid;
			ServiceType = serviceType;
			Protocol = protocol;
		}

		public void Add(IPEndPoint endPoint)
		{
			
		}

		public void Initialize()
		{
			foreach (var socket in Sockets.Values.ToList())
				socket.Initialize();
		}

		public void Start()
		{
			foreach (var socket in Sockets.Values.ToList())
				socket.Start();
		}

		public void Stop()
		{
			foreach (var socket in Sockets.Values.ToList())
				socket.Close();
		}

		public void Dispose()
		{
			foreach (var sockets in Sockets.Values.ToList())
				sockets.Dispose();
		}

		public IPAddress Get_IPAddress(Guid socket)
			=> Sockets[socket].GetIPAddress();

		public void Send(Guid socketId, IPacket packet, IClient client)
			=> Sockets[socketId]?.SendTo(packet, client);
	}
}
