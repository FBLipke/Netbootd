using Netboot.Common.Network.Sockets;
using Netboot.Common.System;
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
						HTTPRequestReceived?.Invoke(this,
							new HTTPRequestReceivedEventArgs(e.Server, e.Socket, e.Client, true, e.Context));
						break;
					case ProtoType.Udp:
						UDPRequestReceived?.Invoke(this, 
							new UDPRequestReceivedEventArgs(e.Server, e.Socket, e.Client, e.Data));
						break;
					default:
						return;
				}

			};
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
