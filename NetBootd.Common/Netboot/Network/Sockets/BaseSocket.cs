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

using Netboot.Network.Sockets.Definition;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace Netboot.Network.Sockets
{
	public class BaseSocket : IDisposable, ISocket
	{
		public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
		public delegate void DataSendEventHandler(object sender, DataSentEventArgs e);
		public event DataReceivedEventHandler? DataReceived;
		public event DataSendEventHandler? DataSent;

		SocketState? socketState;
		EndPoint localendpoint;
		EndPoint remoteendpoint;
		Guid SocketId;
		Guid ServerId;

		bool IsDisposed;

		public string ServiceType;

		public bool Listening { get; private set; }
		public int BufferLength { get; private set; }

		public bool Multicast { get; private set; }

		public BaseSocket(Guid serverId, Guid socketId, string serviceType, IPEndPoint localep, bool multicast = false, int buffersize = ushort.MaxValue)
		{
			localendpoint = localep;
			remoteendpoint = new IPEndPoint(IPAddress.Any, 0);
			BufferLength = buffersize;
			SocketId = socketId;
			ServerId = serverId;
			ServiceType = serviceType;
			Multicast = multicast;

			socketState = new SocketState
			{
				socket = new Socket(localendpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp),
				buffer = new byte[BufferLength]
			};
		}

		public IPAddress GetIPAddress()
			=> ((IPEndPoint)localendpoint).Address;

		public void Start()
		{
			if (socketState == null)
				return;

			try
			{
				socketState.socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				socketState.socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
/*
				if (Multicast)
				{
					socketState.socket?.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
						new MulticastOption(IPAddress.Parse("224.0.1.2")));
				}
*/
				socketState.socket?.Bind(localendpoint);

				socketState.socket?.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
					SocketFlags.None, ref localendpoint, new(EndReceive), socketState);

				Listening = true;
			}
			catch (SocketException ex)
			{
				Console.WriteLine(ex);
				Listening = false;
			}
		}

		public void Close()
		{
			Console.WriteLine($"[I] Closed Socket {SocketId}!");
			socketState.Close();
			Listening = false;
		}

		private void EndReceive(IAsyncResult asyncResult)
		{
			try
			{
				socketState = (SocketState)asyncResult.AsyncState;
				var client = socketState.socket;
				if (client == null)
					return;

				if (socketState == null)
					return;

				var bytesRead = client.EndReceiveFrom(asyncResult, ref remoteendpoint);
				if (bytesRead == 0 || bytesRead == -1)
					return;

				var data = new byte[bytesRead];
				Array.Copy(socketState.buffer, data, data.Length);

				DataReceived?.Invoke(this, new(ServiceType, ServerId, SocketId, data,
					(IPEndPoint)remoteendpoint));

				socketState.socket.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
					SocketFlags.None, ref localendpoint, new(EndReceive), socketState);
			}
			catch (ObjectDisposedException ex)
			{
				Console.WriteLine(ex);
				Listening = false;
			}
			catch (SocketException ex)
			{
				Console.WriteLine(ex);
				Listening = false;
			}
		}

		public void SendTo(IPacket packet, IClient client)
		{
			var buffer = packet.Buffer.GetBuffer();

			try
			{
				var bytesSent = socketState.socket.SendTo(buffer, 0, (int)packet.Buffer.Length,
					SocketFlags.None, client.RemoteEntpoint);
			}
			catch (SocketException ex)
			{
				Console.WriteLine(ex);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
					socketState?.Dispose();

				socketState = null;
				IsDisposed = true;
			}
		}
	}
}
