using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using Netboot.Services.Interfaces;
using System.Net;
using System.Xml;
using static Netboot.Services.Interfaces.IService;

namespace Netboot.Service.DHCP
{
    public class DHCPService : IService
    {
        public DHCPService(string serviceType)
        {
            ServiceType = serviceType;
        }

        public List<ushort> Ports { get; } = new List<ushort>();

        public string ServiceType { get; }

        public Dictionary<string, IClient> Clients { get; set; } = [];

        public event AddServerEventHandler? AddServer;
		public event ServerSendPacketEventHandler? ServerSendPacket;

		public void Dispose()
        {
            foreach (var client in Clients.Values)
                client.Dispose();

            Ports.Clear();
        }

        void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
        {
            if (!Clients.ContainsKey(clientId))
                Clients.Add(clientId, new DHCPClient(clientId, serviceType, remoteEndpoint, serverId, socketId));
        }

        public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Service: DHCP!");

            var requestPacket = new DHCPPacket(e.ServiceType, e.Packet);

            switch (requestPacket.GetVendorIdent)
            {
                case PXEVendorID.PXEClient:
                case PXEVendorID.PXEServer:
                case PXEVendorID.AAPLBSDPC:
					var clientid = string.Join(":", requestPacket.HardwareAddress.Select(x => x.ToString("X2")));
					AddClient(clientid, e.ServiceType, e.RemoteEndpoint, e.ServerId, e.SocketId);

					Console.WriteLine("Got Request from: {0}", Clients[clientid].RemoteEntpoint);

					switch (requestPacket.BootpOPCode)
					{
						case BOOTPOPCode.BootRequest:
							switch ((DHCPMessageType)requestPacket.GetOption(53).Data[0])
							{
								case DHCPMessageType.Discover:
									Clients[clientid].RemoteEntpoint.Address = IPAddress.Broadcast;
									Handle_DHCP_Discover(e.ServerId, e.SocketId, clientid, requestPacket);
									break;
								case DHCPMessageType.Request:
									break;
								case DHCPMessageType.Inform:
									break;
							}

							break;
						case BOOTPOPCode.BootReply:
							Console.WriteLine("BOOTP: Reply!");
							break;
						default:
							break;
					}
					break;
				case PXEVendorID.None:
				case PXEVendorID.Msft:
				default:
					return;
			}







		}

        public void Handle_DataSent(object sender, DataSentEventArgs e)
        {
            Console.WriteLine(e.RemoteEndpoint);
        }

        public bool Initialize(XmlNode xmlConfigNode)
        {
			var ports = xmlConfigNode.Attributes.GetNamedItem("port").Value.Split(',').ToList();
            if (ports.Count > 0)
            {
				foreach (var port in ports)
                    Ports.Add(ushort.Parse(port.Trim()));
			}

            AddServer?.Invoke(this, new(ServiceType, Ports));
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void Handle_DHCP_Discover(Guid server, Guid socket, string client, DHCPPacket packet)
        {
            var serverIP = NetbootBase.Servers[server].Get_IPAddress(socket);
            var response = packet.CreateResponse(serverIP);

            response.CommitOptions();
            ServerSendPacket.Invoke(this, new(server, socket, response, Clients[client]));
        }

		private void Handle_DHCP_Request(Guid server, Guid socket, DHCPPacket packet)
		{
		}

		public void Heartbeat()
		{
            Console.WriteLine("Heartbeat...");
		}
	}
}
