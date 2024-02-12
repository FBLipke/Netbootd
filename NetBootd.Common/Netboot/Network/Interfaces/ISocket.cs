using System.Net;

namespace Netboot.Network.Interfaces
{
	public interface ISocket : IDisposable
	{
		void Start();
		void SendTo(IPacket packet, IClient client);
		void Close();
		IPAddress GetIPAddress();

	}
}
