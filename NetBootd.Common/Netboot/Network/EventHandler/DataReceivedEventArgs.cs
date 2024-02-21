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

using System.Net;

namespace Netboot.Network.EventHandler
{
	public class DataReceivedEventArgs
	{
		public byte[] Packet { get; private set; }
		public IPEndPoint RemoteEndpoint { get; private set; }
		public Guid SocketId { get; private set; }
		public Guid ServerId { get; private set; }
		public string ServiceType { get; private set; }

		public DataReceivedEventArgs(string serviceType, Guid serverId,
			Guid socketId, byte[] packet, IPEndPoint remoteEndpoint)
		{
			ServiceType = serviceType;
			ServerId = serverId;
			SocketId = socketId;
			Packet = packet;
			RemoteEndpoint = remoteEndpoint;
		}
	}
}