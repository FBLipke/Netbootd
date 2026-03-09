using Netboot.Common.Network.Sockets;
using Netboot.Common.Network.sockets.Interfaces;

namespace Netboot.Common.Network.sockets
{
	public enum ServerMode
	{
		None,
		Http,
		HttpMedia,
		Udp
	}

	public delegate void ServerAddedSocketEventHandler(INetbootServer sender, ServerAddedSocketArgs e);

	public delegate void ServerClosedSocketEventHandler(
	  INetbootServer sender,
	  ServerClosedSocketArgs e);

	public delegate void ServerClosedClientConnectionEventHandler(
	  INetbootServer sender,
	  ServerClosedClientConnectionArgs e);


	public delegate void ServerReceivedDataEventHandler(INetbootServer sender, ServerReceivedDataArgs e);

}
