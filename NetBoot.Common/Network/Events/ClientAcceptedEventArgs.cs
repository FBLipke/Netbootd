using Netboot.Common.Network.Sockets.Interfaces;

namespace Netboot.Common.Network.Sockets
{
	public class ClientAcceptedEventArgs
	{
		public ClientAcceptedEventArgs(INetbootClient client)
			=> Client = client;

		public INetbootClient Client { get; private set; }
	}
}
