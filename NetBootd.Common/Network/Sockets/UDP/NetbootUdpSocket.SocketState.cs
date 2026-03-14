using System.Net.Sockets;

namespace Netboot.Common.Network.Sockets
{
	public class SocketState
	{
		public byte[] Buffer;
		public int Buffersize;
		public Socket Socket;
		public int Length;
		public SocketType Type;
	}
}
