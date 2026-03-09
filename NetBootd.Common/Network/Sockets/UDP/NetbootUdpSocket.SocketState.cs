using System.Net.Sockets;

namespace Netboot.Common.Network.sockets.UDP
{
	public partial class NetbootUdpSocket
	{
		protected class SocketState
		{
			public byte[] Buffer;
			public int Buffersize;
			public Socket Socket;
			public int Length;
			public SocketType Type;
		}
	}
}
