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
		Guid SocketId;
		Guid ServerId;

		bool IsDisposed;

		public string ServiceType;

		public bool Listening { get; private set; }

		public SocketProtocol Protocol { get; private set; }

		public int BufferLength { get; private set; }

		public bool Multicast { get; private set; }

		public BaseSocket(Guid serverId, Guid socketId, string serviceType, SocketProtocol protocol, IPEndPoint localep, bool multicast = false, int buffersize = ushort.MaxValue)
		{
			localendpoint = localep;
			BufferLength = buffersize;
			SocketId = socketId;
			ServerId = serverId;
			ServiceType = serviceType;
			Multicast = multicast;
			Protocol = protocol;
	
			socketState = new SocketState
			{
				socket = new(localendpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp),
				buffer = new byte[BufferLength]
			};
		}

		public IPAddress GetIPAddress()
			=> ((IPEndPoint)localendpoint).Address;

		public async void Start()
		{
			if (socketState == null)
				return;

			try
			{
				switch (Protocol)
				{
					case SocketProtocol.RAW:
					case SocketProtocol.UDP:
						socketState.socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
						socketState.socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

						if (Multicast)
							socketState.socket?.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
								new MulticastOption(IPAddress.Parse("224.0.1.2")));

						socketState.socket?.Bind(localendpoint);
						Listening = true;

						try
						{
							while (Listening)
							{
								var socketTask = await socketState.socket.ReceiveFromAsync
									(socketState.buffer, new IPEndPoint(IPAddress.Any, 0));

								if (socketTask.ReceivedBytes == 0 || socketTask.ReceivedBytes == -1)
									continue;

								DataReceived?.Invoke(this, new(false, ServiceType, ServerId, 
									SocketId, socketState.buffer.ToArray(), (IPEndPoint)socketTask.RemoteEndPoint));
							}
						}
						catch (SocketException ex)
						{
							Listening = false;
							Console.WriteLine(ex);
						}
						
						break;
					default:
						break;
				}

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

		public void SendTo(IPacket packet, IClient client)
		{
			switch (Protocol)
			{
				case SocketProtocol.RAW:
				case SocketProtocol.UDP:
					var buffer = packet.Buffer.GetBuffer();

					if (client.RemoteEndpoint.Address.Equals(IPAddress.Any))
						client.RemoteEndpoint.Address = IPAddress.Broadcast;

					var bytesSent = socketState.socket.SendTo(buffer, 0, (int)packet.Buffer.Length,
						SocketFlags.None, client.RemoteEndpoint);
					break;
				default:
					break;
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
