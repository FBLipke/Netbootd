using Netboot.Common.Netboot.Network.EventHandler;
using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using Netboot.Services.Interfaces;
using System.Net;
using static Netboot.Services.Interfaces.IService;

namespace Netboot.Service.DHCP
{
    public class DHCPService : IService
    {
        public DHCPService(string serviceType)
        {
            ServiceType = serviceType;
        }

        public List<ushort> Ports => new List<ushort> { 67, 4011 };

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
            var clientid = string.Join("-", requestPacket.HardwareAddress.Select(x => x.ToString("X2")));
            AddClient(clientid, e.ServiceType,e.RemoteEndpoint, e.ServerId, e.SocketId);

            Console.WriteLine("Got Request from: {0}", Clients[clientid].RemoteEntpoint);

            switch (requestPacket.BootpOPCode)
            {
                case BOOTPOPCode.BootRequest:
                    Console.WriteLine("BOOTP: Request!");

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
        }

        public void Handle_DataSent(object sender, DataSentEventArgs e)
        {
            Console.WriteLine(e.RemoteEndpoint);
        }

        public bool Initialize()
        {
            AddServer?.Invoke(this, new AddServerEventArgs(ServiceType, Ports));
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
            ServerSendPacket.Invoke(this,new ServerSendPacketEventArgs(server, socket, response, Clients[client]));
            Console.WriteLine("Handle_DHCP_Discover -> Done");
        }

		private void Handle_DHCP_Request(Guid server, Guid socket, DHCPPacket packet)
		{
		}
	}
}
