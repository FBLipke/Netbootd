using Netboot.Common.Network.Sockets;
using Netboot.Common.Network.sockets.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netboot.Common.Network.sockets.UDP
{
	public class NetbootUdpClient : IDisposable, INetbootClient
	{
		public event ClientErrorEventHandler ClientError;

		public ushort ClientBuffer { get; private set; }
		
		public bool Connected { get; set; }
		
		public Guid Id { get; set; }

		public IPEndPoint RemoteEndpoint { get; set; }

		public NetbootUdpClient(Guid id, IPEndPoint remoteEndpoint, ushort clientBuffer = 4096)
		{
			Id = id;
			Connected = true;
			ClientBuffer = clientBuffer;
			RemoteEndpoint = remoteEndpoint;
		}
		
		public void Close()
		{
			Connected = false;
		}

		public void Dispose()
		{
		}

		public void Send(IPEndPoint remoteEndpoint, ref byte[] data)
		{
		}

		public void Send(ref byte[] data)
		{
		}

		public void Send(IPEndPoint remoteEndpoint, ref MemoryStream data)
		{
		}

		public void Send(ref MemoryStream data)
		{
		}

		public void HeartBeat()
		{
		}

		public IPEndPoint GetEndPoint()
		{
			return RemoteEndpoint;
		}

		public void Start()
		{
		}

		public void Disconnect()
		{
		}

		public void Send(string data, bool keepAlive)
		{
		}

		public void Send(MemoryStream data, bool keepAlive)
		{
		}

		public void Send(string data, Encoding encoding, bool keepAlive)
		{
		}

		public void Send(ref byte[] data, bool keepalive)
		{
		}

		public void Read()
		{
			throw new NotImplementedException();
		}

		public delegate void DataReadFromClientEventHandler(object sender, DataReadFromClientEventArgs e);
		public delegate void ClientErrorEventHandler(object sender, ClientErrorEventArgs e);
		public delegate void ClientClosedEventHandler(object sender, ClientConnectionClosedEventArgs e);
		public delegate void ClientConnectedEventHandler(object sender, ClientConnectedEventArgs e);
	}
}
