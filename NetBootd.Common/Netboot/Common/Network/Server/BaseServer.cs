using Netboot.Common.Network.Interfaces;
using NetBoot;
using NetBoot.Common.Netboot.Common.Network.Definitions;
using NetBoot.Common.Netboot.Common.Network.Interfaces;
using System.Net;
using System.Net.NetworkInformation;

namespace Netboot.Common.Network.Sockets.Server
{
	public class BaseServer : IServer
	{
		public Dictionary<Guid, ISocket> Sockets = [];
		public Guid ServerId = Guid.Empty;
		public ServerType ServerType;

		public BaseServer(Guid serverid, ServerType serverType, ushort port)
		{
			ServerId = serverid;
			ServerType = serverType;

			var addresses = Functions.GetIPAddresses();

			foreach (var address in addresses)
			{
				var socketID = Guid.NewGuid();
				Console.WriteLine($"\tAdded {ServerType} Socket: {socketID} : {address}");

				var socket = new BaseSocket(socketID, ServerType, new IPEndPoint(address, port));

				socket.DataSent += (sender, e) => {
					Console.WriteLine($"{e.BytesSent} bytes sent to {e.RemoteEndpoint}");
				};

				socket.DataReceived += (sender, e) => {
					Console.WriteLine($"Got {e.Data.Length} bytes from {e.RemoteEndpoint}!");
				};

				Sockets.Add(socketID, socket);
			}
		}

		public void Start()
		{
			foreach (var Sockets in Sockets)
				Sockets.Value.Start();
		}

		public void Stop()
		{
			foreach (var Sockets in Sockets)
				Sockets.Value.Close();
		}

		public void Dispose()
		{
			foreach (var Sockets in Sockets)
				Sockets.Value.Dispose();
		}
	}
}
