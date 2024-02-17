using System.Net;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Sockets;
using System.Linq;
using Netboot.Common;

namespace Netboot.Network.Server
{
	public class BaseServer : IServer
	{
		public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
		public delegate void DataSentEventHandler(object sender, DataSentEventArgs e);
		public event DataReceivedEventHandler? DataReceived;
		public event DataSentEventHandler? DataSent;

		Dictionary<Guid, ISocket> Sockets = [];
		public Guid ServerId;
		public string ServiceType { get; }

		public BaseServer(Guid serverid, string serviceType, IEnumerable<ushort> ports)
		{
			ServerId = serverid;
			ServiceType = serviceType;

			var addresses = Functions.GetIPAddresses();
			foreach (var (address, port) in from address in addresses
				from port in ports select (address, port))
			{
				Add(new(address, port));
			}
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

			Sockets.Add(socketID, socket);
		}

		public void Start()
		{
			foreach (var socket in Sockets.Values.ToList())
				socket.Start();
		}

		public void Stop()
		{
			foreach (var socket in Sockets.Values.ToList())
				socket.Close();
		}

		public void Dispose()
		{
			foreach (var sockets in Sockets.Values.ToList())
				sockets.Dispose();
		}

		public IPAddress Get_IPAddress(Guid socket)
			=> Sockets[socket].GetIPAddress();

		public void Send(Guid socketId, IPacket packet, IClient client)
		{
			Sockets[socketId]?.SendTo(packet, client);
		}
	}
}
