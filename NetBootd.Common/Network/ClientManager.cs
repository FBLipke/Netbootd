using Netboot.Common.Network.HTTP;
using Netboot.Common.Network.Sockets;
using Netboot.Common.System;
using Netboot.Common;
using Netboot.Common.Network.sockets.Interfaces;
using Netboot.Common.Network.sockets.TCP;
using System;
using System.Collections.Generic;

namespace Netboot.Common.Network
{
	public class ClientManager : IManager
	{
		public event ClientManagerReceivedDataEventHandler ClientManagerReceivedData;

		public event ClientManagerClosedConnectionEventHandler ClientManagerClosedConnection;

		public Dictionary<Guid, INetbootClient> Clients { get; }

		public Filesystem FileSystem { get; set; }

		public ClientManager()
		{
			Clients = [];
			HttpRequest httpRequest = new()
			{
				Method = "POST",
				Path = "/api",
				Version = "HTTP/1.1"
			};
			httpRequest.Headers.Add("User-Agent", "Mozilla/5.0");
			httpRequest.Headers.Add("Accept", "*/*");
			httpRequest.Headers.Add("Content-Encoding", "application/x-www-form-urlencoded; charset=utf-8");
			httpRequest.Headers.Add("Connection", "close");
			httpRequest.Create();
		}

		public void Add(string host, ushort port)
		{
			var key = Guid.NewGuid();
			var NetbootTcpClient = new NetbootTcpClient(Guid.NewGuid(), host, port);

			NetbootTcpClient.DataReadFromClient += (sender, e) =>
			{
				var managerReceivedData = ClientManagerReceivedData;

				managerReceivedData(this, new ClientManagerReceivedDataArgs(e.Client, e.Data));
			};
			NetbootTcpClient.ClientError += (sender, e) =>
			{
				if (!Clients.ContainsKey(e.Client))
				{
					NetbootBase.Log("W", "Netboot.ClientManager", string.Format("Client ({0}) requested but does not exist anymore!", e.Client));
				}
				else
				{
					NetbootBase.Log("W", "Netboot.ClientManager", string.Format("Client ({0}) removed due to errors!", e.Client));
					Clients.Remove(e.Client);
					ClientManagerClosedConnection.DynamicInvoke(this, new ClientManagerClosedConnectionArgs(e.Client));
				}
			};

			if (!Clients.ContainsKey(key))
				Clients.Add(key, NetbootTcpClient);

			NetbootTcpClient.ClientClosedConnection += (sender, e) =>
			{
				NetbootBase.Log("W", "Netboot.ClientManager", string.Format("Client ({0}) removed!", e.Client));
				Clients.Remove(e.Client);
			};

			Start();
		}

		public void Close()
		{
			foreach (var NetbootTcpClient in Clients.Values)
				NetbootTcpClient.Close();
		}

		public void Dispose()
		{
			foreach (var NetbootTcpClient in Clients.Values)
				NetbootTcpClient.Dispose();
		}

		public void HeartBeat()
		{
			foreach (var NetbootTcpClient in Clients.Values)
				NetbootTcpClient.HeartBeat();
		}

		public void Start()
		{
			foreach (var NetbootTcpClient in Clients.Values)
				NetbootTcpClient.Start();
		}

		public void Stop()
		{
			foreach (var NetbootTcpClient in Clients.Values)
				NetbootTcpClient.Disconnect();
		}

		public void Bootstrap() => throw new NotImplementedException();

		public delegate void ClientManagerReceivedDataEventHandler(
		  IManager sender,
		  ClientManagerReceivedDataArgs e);

		public delegate void ClientManagerClosedConnectionEventHandler(
		  IManager sender,
		  ClientManagerClosedConnectionArgs e);
	}
}
