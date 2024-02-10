using NetBoot.Common.Netboot.Common.Network.Definitions;
using System.Net;
using System.Net.Sockets;

namespace Netboot.Common.Network.Sockets
{
    internal class SocketState : IDisposable
    {
        public Socket socket;
        public byte[] buffer;

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
            Array.Clear(buffer, 0, buffer.Length);
        }
    }

    public class BaseSocket : IDisposable
    {
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
        public delegate void DataSendEventHandler(object sender, DataSendEventArgs e);
        public event DataReceivedEventHandler? DataReceived;
        public event DataSendEventHandler? DataSent;

        SocketState socketState;
        EndPoint localendpoint;
        Guid SocketId;
		public ServerType ServerType;

		public bool Listening { get; private set; }
        public int BufferLength { get; private set; }

        public BaseSocket(Guid socketId, ServerType serverType, IPEndPoint localep, int buffersize = ushort.MaxValue)
        {
            localendpoint = localep;
            BufferLength = buffersize;
            SocketId = socketId;
            ServerType = serverType;


			socketState = new SocketState
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                buffer = new byte[BufferLength]
            };
		}

        public void Start()
        {
            try
            {
				socketState.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				socketState.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
				socketState.socket.Bind(localendpoint);
				socketState.socket.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
                    SocketFlags.None, ref localendpoint, new AsyncCallback(EndReceive), socketState);

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
        }

        private void EndReceive(IAsyncResult asyncResult)
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

            DataReceived?.Invoke(this, new DataReceivedEventArgs(SocketId, data, (IPEndPoint)localendpoint));

			socketState.socket.BeginReceiveFrom(socketState.buffer, 0, socketState.buffer.Length,
                SocketFlags.None, ref localendpoint, new AsyncCallback(EndReceive), socketState);
		}

        public void SendTo(byte[] buffer, IPEndPoint endpoint)
        {
            try
            {
                socketState.socket.BeginSendTo(buffer, 0, buffer.Length, 
                    SocketFlags.None, endpoint, EndSendTo, socketState);
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

            DataSent?.Invoke(this, new DataSendEventArgs(SocketId, bytesSent, remoteEndpoint));
        }

        public void Dispose()
        {
            socketState.Dispose();
        }
    }
}
