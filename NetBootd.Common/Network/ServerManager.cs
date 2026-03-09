using Netboot.Common.Network.Sockets;
using Netboot.Common.Network.sockets;
using Netboot.Common.Network.sockets.Interfaces;
using Netboot.Common.Network.sockets.TCP;
using Netboot.Common.Network.sockets.UDP;
using Netboot.Common.System;
using System.Text;
using System.Net;

namespace Netboot.Common.Network
{
	public class ServerManager : IManager
	{
		public event ReceivedDataEventHandler ReceivedData;

		public Dictionary<Guid, INetbootServer> Servers { get; private set; } = [];

		public Filesystem FileSystem
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public IPEndPoint GetClientEndPoint(Guid server, Guid socket, Guid client)
		{
			return Servers[server].Sockets[socket].Clients[client].RemoteEndpoint;
		}

		public ServerManager() { }

		public void Start()
		{
			foreach (KeyValuePair<Guid, INetbootServer> server in Servers)
				server.Value.Start();
		}

		public void Send(Guid server, Guid socket, Guid client, byte[] data)
		{
			lock (Servers)
			{
				if (Servers.ContainsKey(server))
					if (Servers[server].Sockets.ContainsKey(socket))
						Servers[server]?.Sockets[socket]?.Send(client, data);
			}
		}

		public void Send(Guid server, Guid socket, Guid client, byte[] data, bool keepalive)
		{
			lock (Servers)
			{
				Servers[server].Sockets[socket].Send(client, data, keepalive);
			}
		}

		public void Send(Guid server, Guid socket, Guid client, MemoryStream data, bool keepalive)
		{
			lock (Servers)
			{
				Servers[server].Sockets[socket].Send(client, data, keepalive);
			}
		}

		public void Send(Guid server, Guid socket, Guid client, string data, Encoding encoding, bool keepalive)
		{
			lock (Servers)
			{
				Servers[server].Sockets[socket].Send(client, data, encoding, keepalive);
			}
		}

		public void Add(ProtoType protocolType, ServerMode mode, List<ushort> port)
		{
			var guid = Guid.NewGuid();

			INetbootServer server;
			switch (protocolType)
			{
				case ProtoType.Tcp:
					server = new NetbootTcpServer(protocolType, guid, mode, port);
					break;
				case ProtoType.Raw:
				case ProtoType.Udp:
					server = new NetbootUdpServer(protocolType, guid, mode, port);
					break;
				default:
					throw new InvalidOperationException(string.Format("Invalid Protocoltype: {0}", protocolType));
			}

			if (!Servers.ContainsKey(guid))
			{
				Servers.Add(guid, server);
				Servers[guid].ServerAddedSocket += (sender, e) =>
				{
					Servers[e.Server].Sockets[e.Socket].Start();

					NetbootBase.Log("I", "ServerManager",
						string.Format("Server '{0}' added Socket '{1}'", e.Server, e.Socket));
				};

				
				Servers[guid].ServerClosedSocket += (Sender, e) =>
				{
					NetbootBase.Log("I", "ServerManager",
						string.Format("Server '{0}' closed Socket '{1}'", e.Server, e.Socket));
				};

				Servers[guid].ServerClosedClientConnection += (sender, e) =>
				{
					server.Sockets[e.Socket].Close(e.Client);

					NetbootBase.Log("I", "ServerManager",
						string.Format("Client '{1}' dropped on Socket '{0}'!", e.Socket, e.Client));
				};

				Servers[guid].ServerReceivedData += (sender, e) =>
				{
					ReceivedData.DynamicInvoke(this, new ReceivedDataArgs(e.Server, e.Socket, e.Client, e.ProtocolType, e.Data));
				};

				Servers[guid].Start();
			}
		}

		public IPEndPoint GetEndPoint(Guid server, Guid socket)
			=> Servers[server].GetEndPoint(socket);

		public IPEndPoint GetClient(Guid server, Guid socket, Guid client)
			=> Servers[server].Sockets[socket].Clients[client].GetEndPoint();

		public void Close()
		{
			lock (Servers)
			{
				foreach (var NetbootServer in Servers.Values)
				{
					lock (NetbootServer)
						NetbootServer.Close();
				}
			}
		}

		public void Stop()
		{
			foreach (var NetbootServer in Servers.Values)
				NetbootServer.Stop();
		}

		public void Dispose()
		{
			foreach (var NetbootServer in Servers.Values)
				NetbootServer.Dispose();

			Servers.Clear();
		}

		public void HeartBeat()
		{
			foreach (var NetbootServer in Servers.Values)
				NetbootServer.HeartBeat();
		}

		public void Bootstrap()
		{
			foreach (var NetbootServer in Servers.Values)
				NetbootServer.Bootstrap();
		}

		public void JoinMulticastGroup(Guid server, Guid socket, IPAddress group)
		{
			Servers[server].Sockets[socket].JoinMulticastGroup(group);
			NetbootBase.Log("I", "ServerManager",
					string.Format("Socket {0} on Server '{1}' joined MulticastGroup {2} with interface Address...", socket, server, group));

		}

		public void LeaveMulticastGroup(Guid server, Guid socket, IPAddress group)
		{
			NetbootBase.Log("I", "ServerManager",
				string.Format("Socket {0} on Server '{1}' left MulticastGroup {2}...", socket, server, group));

			Servers[server].Sockets[socket].LeaveMulticastGroup(group);
		}

		public delegate void ReceivedDataEventHandler(object sender, ReceivedDataArgs e);
	}
}
