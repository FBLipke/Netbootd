using System.Net;

namespace Netboot.Network.Interfaces
{
	public interface IServer : IDisposable
	{
		string ServiceType { get; }

		void Start();
		void Stop();
		void Send(Guid socketId, IPacket packet, IClient client);
		IPAddress Get_IPAddress(Guid socket);
	}
}
