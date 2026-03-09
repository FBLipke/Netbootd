using Netboot.Common.Network;
using Netboot.Common.Network.Sockets;
using Netboot.Common.System;
using Netboot.Common.Network.sockets;
using Netboot.Common.Network.sockets.Interfaces;
using Netboot.Common.System;
using System.Net;
using System.Text;

namespace Netboot.Common.Network.sockets.TCP
{
	public class NetbootTcpServer : IDisposable, IManager, INetbootServer
	{
		public Dictionary<Guid, INetbootSocket> Sockets { get; set; }

		public Guid Id { get; set; }

		public ServerMode ServerMode { get; set; }

		public ProtoType ProtocolType { get; set; }

		public Filesystem FileSystem { get; set; }

		Action<ServerMode, IPEndPoint> YieldFunc => (mode, endp) =>
		{
			Add(mode, endp);
		};

		public NetbootTcpServer(ProtoType protocolType, Guid id, ServerMode mode, List<ushort> ports)
		{
			Id = id;
			ProtocolType = protocolType;
			Sockets = new Dictionary<Guid, INetbootSocket>();
			ServerMode = mode;

			NetworkManager.GetIPAddresses(ServerMode, ports, YieldFunc);
		}

		public event sockets.ServerAddedSocketEventHandler ServerAddedSocket;

		public event sockets.ServerClosedSocketEventHandler ServerClosedSocket;

		public event sockets.ServerClosedClientConnectionEventHandler ServerClosedClientConnection;

		public event sockets.ServerReceivedDataEventHandler ServerReceivedData;

		public void Add(ServerMode mode, IPEndPoint endpoint)
		{
			var guid = Guid.NewGuid();
			var NetbootTcpSocket = new NetbootTcpSocket(guid, endpoint);
			NetbootTcpSocket.SocketAddedClient += (sender, e) => Sockets[e.SocketId].Clients[e.ClientId]?.Read();
			NetbootTcpSocket.SocketFailedToStart += (sender, e) => Remove(e.Socket);
			NetbootTcpSocket.SocketClosedClient += (sender, e) =>
		   {
			   ServerClosedSocket(this, new ServerClosedSocketArgs(Id, e.Socket));
			   ServerClosedClientConnection(this, new ServerClosedClientConnectionArgs(Id, e.Socket, e.Client));
		   };

			NetbootTcpSocket.SocketReadDataFromClient += (sender, e) =>
			{
				ServerReceivedData.DynamicInvoke(this,
					new ServerReceivedDataArgs(mode, ProtocolType, Id, e.Socket, e.Client, e.Data));
			};

			Sockets.Add(guid, NetbootTcpSocket);

			ServerAddedSocket?.DynamicInvoke(this, new ServerAddedSocketArgs(Id, guid));
		}

		public void Remove(Guid socket)
		{
			if (!Sockets.ContainsKey(socket))
				return;

			Sockets.Remove(socket);

			ServerClosedSocket(this, new ServerClosedSocketArgs(Id, socket));
		}

		public void Start()
		{
			foreach (var NetbootTcpSocket in Sockets.Values)
				NetbootTcpSocket.Start();
		}

		public void Close()
		{
			foreach (var NetbootTcpSocket in Sockets.Values)
				NetbootTcpSocket.Close();
		}

		public void Dispose()
		{
			foreach (var NetbootTcpSocket in Sockets.Values)
				NetbootTcpSocket.Dispose();

			Sockets.Clear();
		}

		public void Send(Guid socket, Guid client, string data, Encoding encoding, bool keepAlive)
			=> Sockets[socket].Send(client, data, encoding, keepAlive);

		public void Send(Guid socket, Guid client, MemoryStream data, bool keepAlive)
			=> Sockets[socket].Send(client, data, keepAlive);

		public void Send(Guid socket, Guid client, byte[] data, bool keepAlive)
			=> Sockets[socket].Send(client, data, keepAlive);

		public void Stop()
		{
			foreach (var socket in Sockets.Values)
				socket.Close();
		}

		public void HeartBeat()
		{
			lock (Sockets)
			{
				Guid socket = Guid.Empty;
				if (!Sockets.Values.Where(s => !s.Listening).Any())
					return;
				using (var enumerator = Sockets.Values.Where(s => !s.Listening).GetEnumerator())
				{
					if (enumerator.MoveNext())
						socket = enumerator.Current.Id;
				}
				Remove(socket);
			}
		}

		public IPEndPoint GetEndPoint(Guid socket)
			=> Sockets[socket].GetEndPoint();

		public void Bootstrap() => throw new NotImplementedException();

		public void Send(Guid socket, Guid client, IPEndPoint remoteendpoint, MemoryStream data)
		{
			Sockets[socket].Send(client, remoteendpoint, data);
		}

		public void Send(Guid socket, Guid client, IPEndPoint remoteendpoint, byte[] data)
		{
			Sockets[socket].Send(client, remoteendpoint, data);
		}

		public IPEndPoint GetClientEndPoint(Guid server, Guid socket, Guid client)
		{
			return Sockets[socket].Clients[client].RemoteEndpoint;
		}

		public void JoinMulticastGroup(Guid server, Guid socket, IPAddress group)
		{
			Sockets[socket].JoinMulticastGroup(group);
		}

		public void LeaveMulticastGroup(Guid server, Guid socket, IPAddress group)
		{
			Sockets[socket].LeaveMulticastGroup(group);
		}

		public delegate void ServerReceivedDataEventHandler(
		  INetbootServer sender,
		  ServerReceivedDataArgs e);

		public delegate void ServerAddedSocketEventHandler(
		  INetbootServer sender,
		  ServerAddedSocketArgs e);

		public delegate void ServerClosedSocketEventHandler(
		  INetbootServer sender,
		  ServerClosedSocketArgs e);

		public delegate void ServerClosedClientConnectionEventHandler(
		  INetbootServer sender,
		  ServerClosedClientConnectionArgs e);
	}
}
