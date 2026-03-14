using Netboot.Common.Network.HTTP;
using System;
using System.Net.Sockets;

namespace Netboot.Common.Network.Sockets
{
	public partial class ReceivedDataArgs : EventArgs
	{
		public NetbootHttpContext Context { get; private set; }

		public Guid Server { get; private set; }

		public Guid Socket { get; private set; }

		public Guid Client { get; private set; }
		public ProtoType ProtocolType { get; private set; }

		public byte[] Data { get; private set; }

		public ReceivedDataArgs(Guid server, Guid socket, Guid client, ProtoType protoType, byte[] data)
		{
			Server = server;
			Socket = socket;
			Client = client;
			Data = data;
			ProtocolType = protoType;
		}

		public ReceivedDataArgs(
		  Guid server,
		  Guid socket,
		  Guid client,
		  ProtoType protoType,
		  NetbootHttpContext httpcontext)
		{
			Server = server;
			Socket = socket;
			Client = client;
			ProtocolType = protoType;
			Context = httpcontext;
		}
	}
}
