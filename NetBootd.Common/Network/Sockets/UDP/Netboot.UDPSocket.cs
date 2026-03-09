using Netboot.Common.Network.sockets.Interfaces;
using Netboot.Common.Network.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netboot.Common.Network.sockets.UDP
{
	public partial class NetbootUdpSocket : IDisposable, INetbootSocket
	{
		private delegate void ClientAcceptedEventHandler(object sender, ClientAcceptedEventArgs e);
		public delegate void SocketAddedClientEventHandler(object sender, SocketAddedClientEventArgs e);
		public delegate void SocketFailedToStartEventHandler(object sender, SocketFailedToStartEventArgs e);
		public delegate void SocketClosedClientEventHandler(object sender, SocketClosedClientEventArgs e);
		public delegate void SocketReadDataFromClientEventHandler(object sender, SocketReadDataFromClientArgs e);

		private event ClientAcceptedEventHandler InternalClientAccepted;
		public event SocketAddedClientEventHandler SocketAddedClient;
		public event SocketFailedToStartEventHandler SocketFailedToStart;
		public event SocketClosedClientEventHandler SocketClosedClient;
		public event SocketReadDataFromClientEventHandler SocketReadDataFromClient;

		private Socket _sock;
		public Dictionary<Guid, INetbootClient> Clients { get; set; }
		SocketState state;

		public Guid Id { get; set; }

		EndPoint LocalEndpoint;

		public IPAddress MulticastGroup { get; set; }

		public byte MUlticastTTL { get; set; } = 3;

		public bool Listening { get; set; }
		
		public NetbootUdpSocket(Guid id, IPEndPoint endpoint)
		{
			LocalEndpoint = endpoint;
			_sock = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
			_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);


			Clients = [];
			Id = id;

			InternalClientAccepted += (sender, e) =>
			{
				Clients.Add(e.Client.Id, e.Client);

				SocketAddedClient.DynamicInvoke(this, new SocketAddedClientEventArgs(Id, e.Client.Id));
			};
		}

		public void Start()
		{
			try
			{
				state = new SocketState
				{
					Buffer = new byte[1024]
				};

				_sock.Bind(LocalEndpoint);
								
				_sock.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0, ref LocalEndpoint,
					new AsyncCallback(Received), state);

				Listening = true;
			}
			catch (SocketException ex)
			{
				SocketFailedToStart.DynamicInvoke(this, new SocketFailedToStartEventArgs(Id, ex));
			}
		}

		public void JoinMulticastGroup(IPAddress group)
		{
			MulticastGroup = group;

			_sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
				new MulticastOption(MulticastGroup, ((IPEndPoint)LocalEndpoint).Address));
		}

		public void LeaveMulticastGroup(IPAddress group)
		{
			_sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership,
				new MulticastOption(group, ((IPEndPoint)LocalEndpoint).Address));

			MulticastGroup = IPAddress.None;
		}

		private void Received(IAsyncResult ar)
		{
			if (_sock == null)
				return;

			if (!Listening)
				return;

			EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

			state = (SocketState)ar.AsyncState;
			var bytesRead = _sock.EndReceiveFrom(ar, ref remoteEndpoint);
			if (bytesRead == 0 || bytesRead == -1)
				return;

			var data = new byte[bytesRead];
			Array.Copy(state.Buffer, data, data.Length);

			var client = new NetbootUdpClient(Guid.NewGuid(), (IPEndPoint)remoteEndpoint);
			InternalClientAccepted?.DynamicInvoke(this, new ClientAcceptedEventArgs(client));

			SocketReadDataFromClient?.DynamicInvoke(this, new SocketReadDataFromClientArgs(Id, client.Id, data));

			_sock.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0,
				ref LocalEndpoint, new AsyncCallback(Received), state);
		}

		public void Send(Guid client, byte[] data)
		{
			_sock.BeginSendTo(data, 0, data.Length,SocketFlags.None,
				Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
		}

		public void Close(Guid client)
		{
			if (!Clients.ContainsKey(client))
				return;
			Clients[client].Close();
			Remove(client);
		}

		public void Remove(Guid client)
		{
			if (!Clients.ContainsKey(client))
				return;

			Clients.Remove(client);
			var socketClosedClient = SocketClosedClient;

			socketClosedClient?.DynamicInvoke(this, new SocketClosedClientEventArgs(client, Id));
		}

		public void Close()
		{
			Listening = false;
			lock (Clients.Values)
				foreach (var NetbootClient in Clients.Values)
					if (NetbootClient != null)
						lock (NetbootClient)
							NetbootClient.Close();
			
			_sock.Close();
		}

		public void HeartBeat()
		{
		}

		private void EndSend(IAsyncResult ar)
		{
			var so = (Socket)ar.AsyncState;

				var bytesSend = so.EndSendTo(ar);

				if (bytesSend == 0 || bytesSend == -1)
					return;
		}

		public void Send(Guid client, MemoryStream data, bool keepAlive)
		{
			_sock.BeginSendTo(data.GetBuffer(), 0, (int)data.Length,
				SocketFlags.None, Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
		}

		public void Send(Guid client, byte[] data, bool keepAlive)
		{
			_sock.BeginSendTo(data, 0, data.Length,
				SocketFlags.None, Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
		}

		public void Send(Guid client, IPEndPoint remoteEndPoint, MemoryStream data)
		{
			_sock.BeginSendTo(data.GetBuffer(), 0, (int)data.Length,
				SocketFlags.None, remoteEndPoint, new AsyncCallback(EndSend), _sock);
		}

		public void Send(Guid client, IPEndPoint remoteEndPoint, byte[] data)
		{
			_sock.BeginSendTo(data, 0, data.Length,
				SocketFlags.None, remoteEndPoint, new AsyncCallback(EndSend), _sock);
		}

		public void Dispose()
		{
			lock (Clients)
			{
				foreach (var NetbootUdpClient in Clients.Values)
					lock (NetbootUdpClient)
						NetbootUdpClient.Dispose();

				Clients.Clear();
			}
			_sock = null;
		}

		public void Send(Guid client, string data, Encoding encoding, bool keepAlive)
		{
			var bytes = encoding.GetBytes(data);
			_sock.BeginSendTo(bytes, 0, bytes.Length,
				SocketFlags.None, Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
		}

		public IPEndPoint GetEndPoint()
		{
			return (IPEndPoint)LocalEndpoint;
		}
	}
}
