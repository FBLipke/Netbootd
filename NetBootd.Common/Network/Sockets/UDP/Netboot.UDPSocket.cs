using Netboot.Common.Network.Sockets.Interfaces;
using Netboot.Common.Network.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netboot.Common.Network.Sockets
{
    public class NetbootUdpSocket : IDisposable, INetbootSocket
    {
        private delegate void ClientAcceptedEventHandler(object sender, ClientAcceptedEventArgs e);
        public delegate void SocketAddedClientEventHandler(object sender, SocketAddedClientEventArgs e);
        public delegate void SocketFailedToStartEventHandler(object sender, SocketFailedToStartEventArgs e);
        public delegate void SocketClosedClientEventHandler(object sender, SocketClosedClientEventArgs e);
        public delegate void SocketReadDataFromClientEventHandler(object sender, SocketReadDataFromClientArgs e);

        private event ClientAcceptedEventHandler InternalClientAccepted;
        public event SocketAddedClientEventHandler SocketAddedClient;
        public event SocketFailedToStartEventHandler SocketFailedToStart;
        public event SocketClosedClientEventHandler SocketClosedClient;
        public event SocketReadDataFromClientEventHandler SocketReadDataFromClient;

        private Socket _sock;
        public Dictionary<Guid, INetbootClient> Clients { get; set; }
        SocketState state;

        public Guid Id { get; set; }

        EndPoint LocalEndpoint;

        public IPAddress MulticastGroup { get; set; }

        public byte MulticastTTL { get; set; } = 3;

        public bool Listening { get; set; }

        public NetbootUdpSocket(Guid id, IPEndPoint endpoint)
        {
            LocalEndpoint = endpoint;
            _sock = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);


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
                state = new SocketState
                {
                    Buffer = new byte[1024]
                };

                _sock.Bind(LocalEndpoint);

                if (joinMulticastGroup)
                    JoinMulticastGroup(MulticastGroup);

                _sock.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0, ref LocalEndpoint,
                    new AsyncCallback(Received), state);

                Listening = true;
            }
            catch (SocketException ex)
            {
                SocketFailedToStart?.Invoke(this, new SocketFailedToStartEventArgs(Id, ex));
            }
        }

        public void JoinMulticastGroup(IPAddress group)
        {
            if (LocalEndpoint.AddressFamily != AddressFamily.InterNetwork)
                return;
            
            MulticastGroup = group;

            _sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                new MulticastOption(MulticastGroup, ((IPEndPoint)LocalEndpoint).Address));

        }

        public void LeaveMulticastGroup(IPAddress group)
        {
            if (LocalEndpoint.AddressFamily != AddressFamily.InterNetwork)
                return;

            _sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership,
                new MulticastOption(group, ((IPEndPoint)LocalEndpoint).Address));

            MulticastGroup = IPAddress.None;
        }

        private void Received(IAsyncResult ar)
        {
            if (_sock == null)
                return;

            if (!Listening)
                return;

            EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

            state = (SocketState)ar.AsyncState;
            var bytesRead = _sock.EndReceiveFrom(ar, ref remoteEndpoint);
            if (bytesRead == 0 || bytesRead == -1)
                return;

            var data = new byte[bytesRead];
            Array.Copy(state.Buffer, data, data.Length);

            var client = new NetbootUdpClient(Guid.NewGuid(), (IPEndPoint)remoteEndpoint);
            InternalClientAccepted?.Invoke(this, new ClientAcceptedEventArgs(client));

            SocketReadDataFromClient?.Invoke(this, new SocketReadDataFromClientArgs(Id, client.Id, data));

            _sock.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0,
                ref LocalEndpoint, new AsyncCallback(Received), state);
        }

        public void Send(Guid client, byte[] data)
        {
            _sock.BeginSendTo(data, 0, data.Length, SocketFlags.None,
                Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
        }

        public void Close(Guid client)
        {
            if (!Clients.TryGetValue(client, out INetbootClient? value))
                return;

            value.Close();
            Remove(client);
        }

        public void Remove(Guid client)
        {
            if (!Clients.ContainsKey(client))
                return;

            Clients.Remove(client);
            var socketClosedClient = SocketClosedClient;

            socketClosedClient?.Invoke(this, new SocketClosedClientEventArgs(client, Id));
        }

        public void Close()
        {
            Listening = false;
            foreach (var NetbootClient in Clients.Values.ToList())
                NetbootClient?.Close();


            LeaveMulticastGroup(MulticastGroup);

            _sock.Close();
        }

        public void HeartBeat()
        {
        }

        private void EndSend(IAsyncResult ar)
        {
            var so = (Socket)ar.AsyncState;

            var bytesSend = so.EndSendTo(ar);

            if (bytesSend == 0 || bytesSend == -1)
                return;
        }

        public void Send(Guid client, MemoryStream data, bool keepAlive)
        {
            _sock.BeginSendTo(data.GetBuffer(), 0, (int)data.Length,
                SocketFlags.None, Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
        }

        public void Send(Guid client, byte[] data, bool keepAlive)
        {
            _sock.BeginSendTo(data, 0, data.Length,
                SocketFlags.None, Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
        }

        public void Send(Guid client, IPEndPoint remoteEndPoint, MemoryStream data)
        {
            _sock.BeginSendTo(data.GetBuffer(), 0, (int)data.Length,
                SocketFlags.None, remoteEndPoint, new AsyncCallback(EndSend), _sock);
        }

        public void Send(Guid client, IPEndPoint remoteEndPoint, byte[] data)
        {
            _sock.BeginSendTo(data, 0, data.Length,
                SocketFlags.None, remoteEndPoint, new AsyncCallback(EndSend), _sock);
        }

        public void Dispose()
        {
            foreach (var NetbootUdpClient in Clients.Values.ToList())
                NetbootUdpClient.Dispose();

            Clients.Clear();
        }

        public void Send(Guid client, string data, Encoding encoding, bool keepAlive)
        {
            var bytes = encoding.GetBytes(data);
            _sock.BeginSendTo(bytes, 0, bytes.Length,
                SocketFlags.None, Clients[client].RemoteEndpoint, new AsyncCallback(EndSend), _sock);
        }

        public IPEndPoint GetEndPoint()
        {
            return (IPEndPoint)LocalEndpoint;
        }
    }
}
