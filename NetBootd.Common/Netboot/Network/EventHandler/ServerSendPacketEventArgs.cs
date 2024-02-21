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

using Netboot.Network.Interfaces;

namespace Netboot.Network.EventHandler
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