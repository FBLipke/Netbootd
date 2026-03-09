using Netboot.Common.Network.sockets.Interfaces;

namespace Netboot.Common.Network.sockets.UDP
{
	public partial class NetbootUdpSocket
	{
		private class ClientAcceptedEventArgs
		{
			public ClientAcceptedEventArgs(INetbootClient client) => Client = client;

			public INetbootClient Client { get; private set; }
		}
	}
}
