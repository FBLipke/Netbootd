using Netboot.Common.System;
using System.Net;
using System.Text;

namespace Netboot.Common.Network.sockets.Interfaces
{
	public interface INetbootServer : IDisposable
	{
		event ServerReceivedDataEventHandler ServerReceivedData;

		event ServerAddedSocketEventHandler ServerAddedSocket;

		event ServerClosedSocketEventHandler ServerClosedSocket;

		event ServerClosedClientConnectionEventHandler ServerClosedClientConnection;

		Dictionary<Guid, INetbootSocket> Sockets { get; set; }

		Guid Id { get; set; }

		ServerMode ServerMode { get; set; }

		ProtoType ProtocolType { get; set; }

		Filesystem FileSystem { get; set; }

		void Add(ServerMode mode, IPEndPoint endpoint);

		void Remove(Guid socket);
		void Start();

		void Close();

		IPEndPoint GetEndPoint(Guid socket);
		public IPEndPoint GetClientEndPoint(Guid server, Guid socket, Guid client);

		void Send(Guid socket, Guid client, string data, Encoding encoding, bool keepAlive);
		void Send(Guid socket, Guid client, MemoryStream data, bool keepAlive);
		void Send(Guid socket, Guid client, byte[] data, bool keepAlive);
		void Send(Guid socket, Guid client, IPEndPoint remoteendpoint, MemoryStream data);
		void Send(Guid socket, Guid client, IPEndPoint remoteendpoint, byte[] data);

		void JoinMulticastGroup(Guid server, Guid socket, IPAddress group);

		void LeaveMulticastGroup(Guid server, Guid socket, IPAddress group);

		void HeartBeat();
		void Stop();
		void Bootstrap();
	}
}
