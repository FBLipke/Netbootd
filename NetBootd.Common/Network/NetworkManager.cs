using Netboot.Common.Network.Sockets;
using Netboot.Common.System;
using Netboot.Common.Network.sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace Netboot.Common.Network
{
	public class NetworkManager : IManager
	{
		public delegate void NetworkManagerRequestHandledEventHandler(IManager sender,
			NetworkManagerRequestHandledEventArgs e);

		public delegate void HTTPRequestReceivedEventHandler(IManager sender, HTTPRequestReceivedEventArgs e);
		public delegate void UDPRequestReceivedEventHandler(IManager sender, UDPRequestReceivedEventArgs e);

		public event HTTPRequestReceivedEventHandler HTTPRequestReceived;
		public event UDPRequestReceivedEventHandler UDPRequestReceived;

		public ServerManager ServerManager { get; }

		public Filesystem FileSystem { get; set; }

		public NetworkManager()
		{
			ServerManager = new ServerManager();
			ServerManager.ReceivedData += (sender, e) =>
			{
				switch (ServerManager.Servers[e.Server].ProtocolType)
				{
					case ProtoType.Tcp:
						switch (ServerManager.Servers[e.Server].ServerMode)
						{
							case ServerMode.HttpMedia:
							case ServerMode.Http:
								HTTPRequestReceived?.DynamicInvoke(this,
									new HTTPRequestReceivedEventArgs(e.Server, e.Socket, e.Client,
										ServerManager.Servers[e.Server].ServerMode == ServerMode.HttpMedia, e.Context));
								break;
							default:
								return;
						}
						break;
					case ProtoType.Udp:
						UDPRequestReceived?.DynamicInvoke(this, 
							new UDPRequestReceivedEventArgs(e.Server, e.Socket, e.Client, e.Data));
						break;
					default:
						return;
				}

			};
		}

		public static void GetIPAddresses(ServerMode mode, List<ushort> ports, Action<ServerMode, IPEndPoint> @delegate)
		{
			foreach (var _port in ports)
			{
				foreach (var networkInterface in NetworkInterface.
					GetAllNetworkInterfaces().Where(adap => adap.GetIPProperties().GatewayAddresses.Count != 0))
					{
						foreach (var unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
							@delegate(mode, new IPEndPoint(unicastAddress.Address, _port));
					}
			}
		}

		public void Close()
		{
			ServerManager.Close();
		}

		public void Start()
		{
			ServerManager.Start();
		}

		public void Stop()
		{
			ServerManager.Stop();
		}

		public void Dispose()
		{
			ServerManager.Dispose();
		}

		public void HeartBeat()
		{
			ServerManager.HeartBeat();
		}

		public void Bootstrap() => throw new NotImplementedException();
	}
}
