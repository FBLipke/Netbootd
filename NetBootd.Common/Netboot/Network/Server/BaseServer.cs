using System.Net;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Sockets;

namespace Netboot.Network.Server
{
    public class BaseServer : IServer
    {
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
        public delegate void DataSendEventHandler(object sender, DataSendEventArgs e);
        public event DataReceivedEventHandler? DataReceived;
        public event DataSendEventHandler? DataSent;

        Dictionary<Guid, ISocket> _Sockets = [];
        public Guid ServerId;
        public ServerType ServerType;

        public BaseServer(Guid serverid, ServerType serverType, ushort port)
        {
            ServerId = serverid;
            ServerType = serverType;

            var addresses = Functions.GetIPAddresses();

            foreach (var address in addresses)
                Add(new(address, port));
        }

        public void Add(IPEndPoint endPoint)
        {
            var socketID = Guid.NewGuid();
            var socket = new BaseSocket(ServerId, socketID, ServerType, endPoint);

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

        public void Send(Guid socketId, IPacket packet, IClient client)
        {
            _Sockets[socketId]
                .SendTo(packet, client);
        }
    }
}
