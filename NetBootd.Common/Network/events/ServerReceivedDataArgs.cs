using System;

namespace Netboot.Common.Network.Sockets
{
	public class ServerReceivedDataArgs : EventArgs
	{
		public ServerReceivedDataArgs(
		  ProtoType protocolType,
		  Guid server,
		  Guid socket,
		  Guid client,
		  byte[] data)
		{
			Server = server;
			Socket = socket;
			Client = client;
			Data = data;
		}

		public Guid Server { get; private set; }

		public ProtoType ProtocolType { get; private set; }

		public Guid Socket { get; private set; }

		public Guid Client { get; private set; }

		public byte[] Data { get; private set; }
	}
}
