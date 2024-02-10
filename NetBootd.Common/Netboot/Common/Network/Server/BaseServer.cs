using NetBoot.Common.Netboot.Common.Network.Definitions;
using System.Net;
using System.Net.NetworkInformation;

namespace Netboot.Common.Network.Sockets.Server
{
	public class BaseServer : IDisposable
	{
		public Dictionary<Guid, BaseSocket> Sockets = [];

		public Guid ServerId = Guid.Empty;
		public ServerType ServerType;

		public BaseServer(Guid serverid, ServerType serverType, ushort port)
		{
			ServerId = serverid;
			ServerType = serverType;

			var addresses = new List<IPAddress>();

			foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
				if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
					foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
						if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
							if (!IPAddress.IsLoopback(ip.Address) && ip.Address.GetAddressBytes()[0] != 0xa9)
								addresses.Add(ip.Address);

			if (addresses.Count == 0)
			{
				Console.WriteLine("[E]: Could not find any suitable network interface!");
				return;
			}

			foreach (var address in addresses)
			{
				var socketID = Guid.NewGuid();
				Console.WriteLine($"\tAdded {ServerType} Socket: {socketID} : {address}");

				var socket = new BaseSocket(socketID, ServerType, new IPEndPoint(address, port));

				socket.DataSent += (sender, e) => {
					Console.WriteLine($"{e.BytesSent} bytes sent to {e.RemoteEndpoint}");
				};

				socket.DataReceived += (sender, e) =>
				{
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
