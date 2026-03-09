using System;

namespace Netboot.Common.Network.Sockets
{
	public class SocketAddedClientEventArgs : EventArgs
	{
		public SocketAddedClientEventArgs(Guid socketId, Guid clientId)
		{
			SocketId = socketId;
			ClientId = clientId;
		}

		public Guid SocketId { get; private set; }

		public Guid ClientId { get; private set; }
	}
}
