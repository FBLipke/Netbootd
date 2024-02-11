using System.Net;
using System.Net.Sockets;
using Netboot.Network.Packet;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;

namespace Netboot.Network.Sockets
{
    internal class SocketState : IDisposable
    {
        public Socket? socket;
        public byte[] buffer = [];

        public SocketState()
        {
        }

        public void Close()
        {
            socket.Close();
        }

        public void Dispose()
        {
            socket.Dispose();

            if (buffer != null)
                Array.Clear(buffer, 0, buffer.Length);
        }
    }

    public class BaseSocket : IDisposable, ISocket
    {
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
        public delegate void DataSendEventHandler(object sender, DataSentEventArgs e);
        public event DataReceivedEventHandler? DataReceived;
        public event DataSendEventHandler? DataSent;

        SocketState socketState;
        EndPoint localendpoint;
        Guid SocketId;
        Guid ServerId;
        public string ServiceType;

        public bool Listening { get; private set; }
        public int BufferLength { get; private set; }

        public BaseSocket(Guid serverId, Guid socketId, string serviceType, IPEndPoint localep, int buffersize = ushort.MaxValue)
        {
            localendpoint = localep;
            BufferLength = buffersize;
            SocketId = socketId;
            ServerId = serverId;
            ServiceType = serviceType;    

            socketState = new SocketState
            {
                socket = new Socket(localendpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp),
                buffer = new byte[BufferLength]
            };
        }

        public void Start()
        {
            if (socketState == null)
                return;

            try
            {
                socketState.socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socketState.socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                socketState.socket?.Bind(localendpoint);

                socketState.socket?.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
                    SocketFlags.None, ref localendpoint, new(EndReceive), socketState);

                Listening = true;

                Console.WriteLine($"[I] Listening on {localendpoint}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
                Listening = false;
            }
        }

        public void Close()
        {
            Console.WriteLine($"[I] Closed Socket {SocketId}!");
            socketState.Close();
            Listening = false;
        }

        private void EndReceive(IAsyncResult asyncResult)
        {
            try
            {
                socketState = (SocketState)asyncResult.AsyncState;
                var client = socketState.socket;
                if (client == null)
                    return;

                var bytesRead = client.EndReceiveFrom(asyncResult, ref localendpoint);
                if (bytesRead == 0 || bytesRead == -1)
                    return;

                var data = new byte[bytesRead];
                Array.Copy(socketState.buffer, data, data.Length);

                IPacket packet = new BasePacket(ServiceType, data);

                DataReceived?.Invoke(this, new(ServiceType, ServerId, SocketId, packet,
                    (IPEndPoint)localendpoint));

                socketState.socket.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
                    SocketFlags.None, ref localendpoint, new(EndReceive), socketState);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
                Listening = false;
            }
        }

        public void SendTo(IPacket packet, IClient client)
        {
            try
            {
                socketState.socket?.BeginSendTo(packet.Buffer.GetBuffer(), 0, (int)packet.Buffer.Length,
                    SocketFlags.None, client.RemoteEntpoint, new(EndSendTo), socketState);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
                Listening = false;
            }
        }

        private void EndSendTo(IAsyncResult ar)
        {
            var socket = ar.AsyncState as Socket;
            if (socket == null)
                return;

            var bytesSent = socket.EndSendTo(ar);
            if (bytesSent == 0)
                return;

            var remoteEndpoint = socket.LocalEndPoint as IPEndPoint;
            if (remoteEndpoint == null)
                return;

            DataSent?.Invoke(this, new(ServiceType, ServerId, SocketId, bytesSent, remoteEndpoint));
        }

        public void Dispose()
        {
            socketState.Dispose();
        }
    }
}
