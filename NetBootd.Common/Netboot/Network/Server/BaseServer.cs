using System.Net;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Sockets;

namespace Netboot.Network.Server
{
	public class BaseServer : IServer
	{
		public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
		public delegate void DataSentEventHandler(object sender, DataSentEventArgs e);
		public event DataReceivedEventHandler? DataReceived;
		public event DataSentEventHandler? DataSent;

		Dictionary<Guid, ISocket> _Sockets = [];
		public Guid ServerId;
		public string ServiceType { get; }

		public BaseServer(Guid serverid, string serviceType, IEnumerable<ushort> ports)
		{
			ServerId = serverid;
			ServiceType = serviceType;

			var addresses = Functions.GetIPAddresses();

			foreach (var address in addresses)
				foreach (var port in ports)
					Add(new(address, port));
		}

		public void Add(IPEndPoint endPoint)
		{
			var socketID = Guid.NewGuid();
			var socket = new BaseSocket(ServerId, socketID, ServiceType, endPoint);

			socket.DataSent += (sender, e) =>
			{
				DataSent.Invoke(this, e);
			};

			socket.DataReceived += (sender, e) =>
			{
				DataReceived.Invoke(this, e);
			};

			_Sockets.Add(socketID, socket);
		}

		public void Start()
		{
			foreach (var Sockets in _Sockets)
				Sockets.Value.Start();
		}

		public void Stop()
		{
			foreach (var Sockets in _Sockets)
				Sockets.Value.Close();
		}

		public void Dispose()
		{
			foreach (var Sockets in _Sockets)
				Sockets.Value.Dispose();
		}

		public IPAddress Get_IPAddress(Guid socket)
			=> _Sockets[socket].GetIPAddress();

		public void Send(Guid socketId, IPacket packet, IClient client)
		{
			_Sockets[socketId]
				.SendTo(packet, client);
		}
	}
}
