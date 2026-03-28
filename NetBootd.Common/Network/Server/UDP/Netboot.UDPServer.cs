using Netboot.Common.Network.Sockets.Interfaces;
using Netboot.Common.System;
using System.Net;
using System.Text;
using System.Xml;

namespace Netboot.Common.Network.Sockets
{
    public class NetbootUdpServer : IDisposable, IManager, INetbootServer
    {
        public Dictionary<Guid, INetbootSocket> Sockets { get; set; }

        public Guid Id { get; set; }

        public ProtoType ProtocolType { get; set; } = ProtoType.Udp;

        public Filesystem FileSystem { get; set; }

        public bool Multicast{ get; private set; }

        Action<IPEndPoint> YieldFunc => (endp) =>
        {
            if (endp.AddressFamily != global::System.Net.Sockets.AddressFamily.InterNetwork)
                return;

            Add(endp);
        };

        public NetbootUdpServer(ProtoType protocolType, Guid id, List<ushort> ports, bool multicast)
        {
            Id = id;
            ProtocolType = protocolType;
            Sockets = [];
            
            Multicast = multicast;

            Functions.GetIPAddresses(ports, YieldFunc);
        }

        public event Sockets.ServerAddedSocketEventHandler ServerAddedSocket;

        public event Sockets.ServerClosedSocketEventHandler ServerClosedSocket;

        public event Sockets.ServerClosedClientConnectionEventHandler ServerClosedClientConnection;

        public event Sockets.ServerReceivedDataEventHandler ServerReceivedData;

        public void Add(IPEndPoint endpoint)
        {
            var guid = Guid.NewGuid();
            var socket = new NetbootUdpSocket(guid, endpoint);
            socket.SocketAddedClient += (sender, e) => { };
            socket.SocketFailedToStart += (sender, e) =>
            {
                NetbootBase.Log("E", this.GetType().ToString(), e.Exception.Message);
                Remove(e.Socket);
            };

            socket.SocketClosedClient += (sender, e) =>
            {
                Remove(e.Socket);
                ServerClosedClientConnection?.Invoke(this, new(Id, e.Socket, e.Client));
            };

            socket.SocketReadDataFromClient += (sender, e) =>
            {
                ServerReceivedData?.Invoke(this,
                    new(ProtocolType, Id, e.Socket, e.Client, e.Data));
            };

            Sockets.Add(guid, socket);
            ServerAddedSocket?.Invoke(this, new(Id, socket.Id));
        }

        public void Remove(Guid socket)
        {
            if (!Sockets.ContainsKey(socket))
                return;

            Sockets.Remove(socket);

			ServerClosedSocket?.Invoke(this, new(Id, socket));
		}

        public void Start()
        {
            foreach (var NetbootUdpSocket in Sockets.Values.ToList())
            {
                NetbootUdpSocket.Start(Multicast);
            }
        }

        public void Close()
        {
            foreach (var NetbootUdpSocket in Sockets.Values.ToList())
                NetbootUdpSocket.Close();
        }

        public void Dispose()
        {
            foreach (var NetbootUdpSocket in Sockets.Values.ToList())
                NetbootUdpSocket.Dispose();

            Sockets.Clear();
        }

        public void Send(Guid socket, Guid client, string data, Encoding encoding, bool keepAlive)
        {
			if (!Sockets.ContainsKey(socket))
				return;

			Sockets[socket].Send(client, data, encoding, keepAlive);
        }

        public void Send(Guid socket, Guid client, MemoryStream data, bool keepAlive)
        {
			if (!Sockets.ContainsKey(socket))
				return;

			Sockets[socket].Send(client, data, keepAlive);
        }

		public void Send(Guid socket, Guid client, IPEndPoint remoteendpoint, MemoryStream data)
		{
			if (!Sockets.ContainsKey(socket))
				return;

			Sockets[socket].Send(client, remoteendpoint, data);
		}

		public void Send(Guid socket, Guid client, IPEndPoint remoteendpoint, byte[] data)
		{
			if (!Sockets.ContainsKey(socket))
				return;

			Sockets[socket].Send(client, remoteendpoint, data);
		}

		public void Send(Guid socket, Guid client, byte[] data, bool keepAlive)
        {
            if (!Sockets.ContainsKey(socket))
                return;
            
            Sockets[socket].Send(client, data, keepAlive);
		}    

        public void Stop()
        {
            foreach (var socket in Sockets.Values.ToList())
                socket.Close();
        }

        public void HeartBeat()
        {
            var socket = Guid.Empty;
            if (!Sockets.Values.Any(s => !s.Listening))
                return;

            using (var enumerator = Sockets.Values.ToList().Where(s => !s.Listening).GetEnumerator())
            {
                if (enumerator.MoveNext())
                    socket = enumerator.Current.Id;
            }

            Remove(socket);
        }

        public void Bootstrap(XmlNode xml)
        {

        }


        public IPEndPoint GetEndPoint(Guid socket)
        {
            return Sockets[socket].GetEndPoint();
        }



        public IPEndPoint GetClientEndPoint(Guid server, Guid socket, Guid client)
        {
            return Sockets[socket].Clients[client].RemoteEndpoint;
        }

        public void JoinMulticastGroup(Guid server, Guid socket, IPAddress group)
        {
			if (!Sockets.ContainsKey(socket))
				return;

			Sockets[socket].JoinMulticastGroup(group);
        }

        public void LeaveMulticastGroup(Guid server, Guid socket, IPAddress group)
        {
			if (!Sockets.ContainsKey(socket))
				return;

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
