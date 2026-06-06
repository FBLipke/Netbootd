using Netboot.Common.Network.Sockets.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netboot.Common.Network.Sockets
{
	public class NetbootTcpSocket : IDisposable, INetbootSocket
	{
		private TcpListener _sock;

		private event ClientAcceptedEventHandler InternalClientAccepted;

		public event SocketAddedClientEventHandler SocketAddedClient;

		public event SocketFailedToStartEventHandler SocketFailedToStart;

		public event SocketClosedClientEventHandler SocketClosedClient;

		public event SocketReadDataFromClientEventHandler SocketReadDataFromClient;

		public Dictionary<Guid, INetbootClient> Clients { get; set; }

		public Guid Id { get; set; }

		IPEndPoint LocalEndpoint;

		public bool Listening { get; set; }
		public IPAddress MulticastGroup { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public byte MulticastTTL { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public NetbootTcpSocket(Guid id, IPEndPoint endpoint)
		{
			LocalEndpoint = endpoint;
			_sock = new TcpListener(LocalEndpoint);

			Clients = [];
			Id = id;

			InternalClientAccepted += (sender, e) =>
			{
				Clients.Add(e.Client.Id, e.Client);

				SocketAddedClient?.Invoke(this, new SocketAddedClientEventArgs(Id, e.Client.Id));
			};
		}

		public void Start(bool joinMulticastGroup)
		{
			try
			{
				_sock.Start();
				_sock.BeginAcceptTcpClient(new AsyncCallback(WaitForClients), null);
				Listening = true;

			}
			catch (SocketException ex)
			{
				SocketFailedToStart?.Invoke(this, new SocketFailedToStartEventArgs(Id, ex));
			}
		}

		private void WaitForClients(IAsyncResult ar)
		{
			if (_sock == null || !Listening)
				return;

			var client = new NetbootTcpClient(Guid.NewGuid(), _sock.EndAcceptTcpClient(ar));
			client.ClientClosedConnection += (sender, e) => Remove(e.Client);
			client.ClientError += (sender, e) => Remove(e.Client);
			client.DataReadFromClient += (sender, e) =>
				SocketReadDataFromClient?.Invoke
					(this, new SocketReadDataFromClientArgs(Id, e.Client, e.Data));

			InternalClientAccepted?.Invoke(this, new ClientAcceptedEventArgs(client));
			_sock.BeginAcceptTcpClient(new AsyncCallback(WaitForClients), null);
		}

		public void Send(Guid client, string data, Encoding encoding, bool keepAlive)
			=> Clients[client].Send(data, encoding, keepAlive);

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

			SocketClosedClient?.Invoke(this, new SocketClosedClientEventArgs(client, Id));
		}

		public void Close()
		{
			Listening = false;

			foreach (var NetbootTcpClient in Clients.Values.ToList())
				NetbootTcpClient?.Close();

			_sock.Stop();
		}

		public void HeartBeat()
		{
			foreach (var NetbootTcpClient in Clients.Values.ToList())
				if (!NetbootTcpClient.Connected)
					Clients.Remove(NetbootTcpClient.Id);
		}

		public void Send(Guid client, MemoryStream data, bool keepAlive)
		{
			if (Clients.ContainsKey(client))
				Clients[client].Send(data, keepAlive);
			else
				Clients.Remove(client);
		}

		public void Send(Guid client, byte[] data, bool keepAlive)
		{
			if (Clients.ContainsKey(client))
				Clients[client].Send(ref data, keepAlive);
			else
				Clients.Remove(client);
		}

		public void Dispose()
		{
			foreach (var NetbootTcpClient in Clients.Values.ToList())
				NetbootTcpClient.Dispose();

			Clients.Clear();
		}

		public void Send(Guid client, byte[] data)
			=> Send(client, data, false);

		public IPEndPoint GetEndPoint() => LocalEndpoint;

		public void Send(Guid client, IPEndPoint remoteEndPoint, byte[] data)
		{
			if (Clients.ContainsKey(client))
				Clients[client].Send(remoteEndPoint, ref data);
			else
				Clients.Remove(client);
		}

		public void Send(Guid client, IPEndPoint remoteEndPoint, MemoryStream data)
		{
			if (Clients.ContainsKey(client))
				Clients[client].Send(remoteEndPoint, ref data);
			else
				Clients.Remove(client);
		}

		public void JoinMulticastGroup(IPAddress group)
		{
			throw new NotImplementedException();
		}

		public void LeaveMulticastGroup(IPAddress group)
		{
			throw new NotImplementedException();
		}

		private delegate void ClientAcceptedEventHandler(
		  INetbootSocket sender,
		  ClientAcceptedEventArgs e);

		public delegate void SocketAddedClientEventHandler(
		  INetbootSocket sender,
		  SocketAddedClientEventArgs e);

		public delegate void SocketFailedToStartEventHandler(
		  INetbootSocket sender,
		  SocketFailedToStartEventArgs e);

		public delegate void SocketClosedClientEventHandler(
		  INetbootSocket sender,
		  SocketClosedClientEventArgs e);

		public delegate void SocketReadDataFromClientEventHandler(
		  INetbootSocket sender,
		  SocketReadDataFromClientArgs e);
	}
}
