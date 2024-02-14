using System.Net;
using System.Net.Sockets;
using Netboot.Common.Properties;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;

namespace Netboot.Network.Sockets
{
	internal class SocketState : IDisposable
	{
		public Socket? socket;
		public byte[] buffer = [];
		private bool IsDisposed;

		public SocketState()
		{
		}

		public void Close()
		{
			socket.Close();
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
				{
					socket?.Dispose();
					if (buffer != null)
						Array.Clear(buffer, 0, buffer.Length);
				}

				socket = null;
				IsDisposed = true;
			}
		}
	}

	public class BaseSocket : IDisposable, ISocket
	{
		public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
		public delegate void DataSendEventHandler(object sender, DataSentEventArgs e);
		public event DataReceivedEventHandler? DataReceived;
		public event DataSendEventHandler? DataSent;

		SocketState socketState;
		EndPoint localendpoint;
		EndPoint remoteendpoint;
		Guid SocketId;
		Guid ServerId;
		
		bool IsDisposed;

		public string ServiceType;

		public bool Listening { get; private set; }
		public int BufferLength { get; private set; }

		public BaseSocket(Guid serverId, Guid socketId, string serviceType, IPEndPoint localep, int buffersize = ushort.MaxValue)
		{
			localendpoint = localep;
			remoteendpoint = new IPEndPoint(IPAddress.Any, 0);
			BufferLength = buffersize;
			SocketId = socketId;
			ServerId = serverId;
			ServiceType = serviceType;

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
				socketState.socket?.Bind(localendpoint);

				socketState.socket?.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
					SocketFlags.None, ref localendpoint, new(EndReceive), socketState);

				Listening = true;

				Console.WriteLine($"[I] Listening on {localendpoint}");
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
			catch (ObjectDisposedException)
			{
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
			Console.WriteLine(packet.Buffer.Length);

			var bytesSent = socketState.socket.SendTo(buffer, 0, (int)packet.Buffer.Length, SocketFlags.None, client.RemoteEntpoint);
			Console.WriteLine("Sent {0} bytes to {1}", bytesSent, client.RemoteEntpoint);
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
