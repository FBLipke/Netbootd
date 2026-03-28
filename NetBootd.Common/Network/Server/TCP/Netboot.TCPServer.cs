using Netboot.Common.Network.Sockets.Interfaces;
using Netboot.Common.System;
using System.Net;
using System.Text;
using System.Xml;

namespace Netboot.Common.Network.Sockets
{
    public class NetbootTcpServer : IDisposable, IManager, INetbootServer
    {
        public Dictionary<Guid, INetbootSocket> Sockets { get; set; }

        public Guid Id { get; set; }

        public ProtoType ProtocolType { get; set; }

        public Filesystem FileSystem { get; set; }

        Action<IPEndPoint> YieldFunc => (endp) =>
        {
            Add(endp);
        };

        public NetbootTcpServer(ProtoType protocolType, Guid id, List<ushort> ports, bool multicast)
        {
            Id = id;
            ProtocolType = protocolType;
            Sockets = [];

            Functions.GetIPAddresses(ports, YieldFunc);
        }

        public event Sockets.ServerAddedSocketEventHandler ServerAddedSocket;

        public event Sockets.ServerClosedSocketEventHandler ServerClosedSocket;

        public event Sockets.ServerClosedClientConnectionEventHandler ServerClosedClientConnection;

        public event Sockets.ServerReceivedDataEventHandler ServerReceivedData;

        public void Add(IPEndPoint endpoint)
        {
            var guid = Guid.NewGuid();
            var NetbootTcpSocket = new NetbootTcpSocket(guid, endpoint);
            NetbootTcpSocket.SocketAddedClient += (sender, e) => Sockets[e.SocketId].Clients[e.ClientId]?.Read();
            NetbootTcpSocket.SocketFailedToStart += (sender, e) => Remove(e.Socket);
            NetbootTcpSocket.SocketClosedClient += (sender, e) => {
               ServerClosedSocket?.Invoke(this, new(Id, e.Socket));
               ServerClosedClientConnection?.Invoke(this, new(Id, e.Socket, e.Client));
            };

            NetbootTcpSocket.SocketReadDataFromClient += (sender, e) =>
            {
                ServerReceivedData?.Invoke(this, new(ProtocolType, Id, e.Socket, e.Client, e.Data));
            };

            Sockets.Add(guid, NetbootTcpSocket);

            ServerAddedSocket?.Invoke(this, new(Id, guid));
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
            foreach (var NetbootTcpSocket in Sockets.Values.ToList())
                NetbootTcpSocket.Start(false);
        }

        public void Close()
        {
            foreach (var NetbootTcpSocket in Sockets.Values.ToList())
                NetbootTcpSocket.Close();
        }

        public void Dispose()
        {
            foreach (var NetbootTcpSocket in Sockets.Values.ToList())
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
            foreach (var socket in Sockets.Values.ToList())
                socket.Close();
        }

        public void HeartBeat()
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

        public IPEndPoint GetEndPoint(Guid socket)
            => Sockets[socket].GetEndPoint();

        public void Bootstrap(XmlNode xml)
        {

        }


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
