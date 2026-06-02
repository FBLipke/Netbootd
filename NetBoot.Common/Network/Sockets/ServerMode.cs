using Netboot.Common.Network.Sockets.Interfaces;

namespace Netboot.Common.Network.Sockets
{
    public delegate void ServerAddedSocketEventHandler(INetbootServer sender, ServerAddedSocketArgs e);
    public delegate void ServerClosedSocketEventHandler(INetbootServer sender, ServerClosedSocketArgs e);

    public delegate void ServerClosedClientConnectionEventHandler(INetbootServer sender, ServerClosedClientConnectionArgs e);
    public delegate void ServerReceivedDataEventHandler(INetbootServer sender, ServerReceivedDataArgs e);

}
