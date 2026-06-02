using Netboot.Common.Network.Sockets;
using Netboot.Common.Network.Sockets.Interfaces;
using Netboot.Common.System;
using System.Net;
using System.Text;
using System.Xml;

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
            foreach (var server in Servers.ToList())
                server.Value.Start();
        }

        public void Send(Guid server, Guid socket, Guid client, byte[] data)
        {
            if (Servers.ContainsKey(server))
                if (Servers[server].Sockets.ContainsKey(socket))
                    Servers[server]?.Sockets[socket]?.Send(client, data);
        }

        public void Send(Guid server, Guid socket, Guid client, byte[] data, bool keepalive)
        {
            if (!Servers.ContainsKey(server))
                return;

            if (!Servers[server].Sockets.ContainsKey(socket))
                return;

            Servers[server].Sockets[socket].Send(client, data, keepalive);
        }

        public void Send(Guid server, Guid socket, Guid client, MemoryStream data, bool keepalive)
        {
            if (!Servers.ContainsKey(server))
                return;

            if (!Servers[server].Sockets.ContainsKey(socket))
                return;

            Servers[server].Sockets[socket].Send(client, data, keepalive);
        }

        public void Send(Guid server, Guid socket, Guid client, string data, Encoding encoding, bool keepalive)
        {
            if (!Servers.ContainsKey(server))
                return;

            if (!Servers[server].Sockets.ContainsKey(socket))
                return;

            Servers[server].Sockets[socket].Send(client, data, encoding, keepalive);
        }

        public void Send(Guid server, Guid socket, Guid client, IPEndPoint endpoint, byte[] bytes)
        {
            if (!Servers.ContainsKey(server))
                return;

            if (!Servers[server].Sockets.ContainsKey(socket))
                return;

            Servers[server].Sockets[socket].Send(client, endpoint, bytes);
        }

        public bool HasSocket(Guid server, Guid Socket)
        {
            if (!Servers.ContainsKey(server))
                return false;

            if (!Servers[server].Sockets.ContainsKey(Socket))
                return false;

            return true;
        }

        public Guid Add(ProtoType protocolType, List<ushort> port, bool multicast = false)
        {
            var guid = Guid.NewGuid();

            INetbootServer server;
            switch (protocolType)
            {
                case ProtoType.Tcp:
                    server = new NetbootTcpServer(protocolType, guid, port, false);
                    break;
                case ProtoType.Raw:
                case ProtoType.Udp:
                    server = new NetbootUdpServer(protocolType, guid, port, false);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Invalid Protocoltype: {0}", protocolType));
            }

            if (!Servers.ContainsKey(guid))
            {
                Servers.Add(guid, server);
                Servers[guid].ServerAddedSocket += (sender, e) =>
                {
                    Servers[e.Server].Sockets[e.Socket].Start(false);

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
                    ReceivedData?.Invoke(this, new ReceivedDataArgs(
                        e.Server, e.Socket, e.Client, e.ProtocolType, e.Data));
                };

                Servers[guid].Start();
            }

            return guid;
        }

        public IPEndPoint GetEndPoint(Guid server, Guid socket)
        {
            return Servers[server].GetEndPoint(socket);
        }

        public IPEndPoint GetClient(Guid server, Guid socket, Guid client)
            => Servers[server].Sockets[socket].Clients[client].GetEndPoint();

        public void Close()
        {
            foreach (var NetbootServer in Servers.Values.ToList())
                NetbootServer.Close();
        }

        public void Stop()
        {
            foreach (var NetbootServer in Servers.Values.ToList())
                NetbootServer.Stop();
        }

        public void Dispose()
        {
            foreach (var NetbootServer in Servers.Values.ToList())
                NetbootServer.Dispose();

            Servers.Clear();
        }

        public void HeartBeat()
        {
            foreach (var NetbootServer in Servers.Values.ToList())
                NetbootServer.HeartBeat();
        }

        public void Bootstrap(XmlNode xml)
        {
            foreach (var NetbootServer in Servers.Values.ToList())
                NetbootServer.Bootstrap(xml);
        }

        public void JoinMulticastGroup(Guid server, IPAddress group)
        {
            if (!Servers.ContainsKey(server))
                return;

            foreach (var socket in Servers[server].Sockets.Values)
            {
                socket.JoinMulticastGroup(group);

                NetbootBase.Log("I", "ServerManager",
                    string.Format("Socket {0} on Server '{1}' joined MulticastGroup {2} with interface address {3}.",
                        socket.Id, server, group, socket.GetEndPoint().Address));
            }
        }

        public void LeaveMulticastGroup(Guid server, Guid socket, IPAddress group)
        {
            if (!Servers.ContainsKey(server))
                return;

            NetbootBase.Log("I", "ServerManager",
                string.Format("Socket {0} on Server '{1}' left MulticastGroup {2}...", socket, server, group));

            Servers[server].Sockets[socket].LeaveMulticastGroup(group);
        }



        public delegate void ReceivedDataEventHandler(object sender, ReceivedDataArgs e);
    }
}
