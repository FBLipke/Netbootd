using Netboot.Common;
using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using Netboot.Services.Interfaces;
using System.Buffers.Binary;
using System.Net;
using System.Xml;
using static Netboot.Services.Interfaces.IService;

namespace Netboot.Services.DHCP
{
	public class DHCPService : IService
	{
		public DHCPService(string serviceType)
		{
			ServiceType = serviceType;
		}

		List<BootServer> bootServers = new List<BootServer>();

		public List<ushort> Ports { get; set; } = [];

		public string ServiceType { get; }

		public Dictionary<string, DHCPClient> Clients { get; set; } = [];

		public event AddServerEventHandler? AddServer;
		public event ServerSendPacketEventHandler? ServerSendPacket;

		ulong i;

		public void Dispose()
		{
			foreach (var client in Clients.Values)
				client.Dispose();

			Ports.Clear();
		}

		void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
		{
			var client = new DHCPClient(clientId, serviceType, remoteEndpoint, serverId, socketId);

			if (!Clients.ContainsKey(clientId))
				Clients.Add(clientId, client);
			else
				Clients[clientId] = client;
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			var requestPacket = new DHCPPacket(e.ServiceType, e.Packet);

			switch (requestPacket.GetVendorIdent)
			{
				case PXEVendorID.PXEClient:
				case PXEVendorID.PXEServer:
				case PXEVendorID.AAPLBSDPC:
					var clientid = string.Join(":", requestPacket.HardwareAddress.Select(x => x.ToString("X2")));
					AddClient(clientid, e.ServiceType, e.RemoteEndpoint, e.ServerId, e.SocketId);
					Console.WriteLine("[I] Got Request from: {0}", Clients[clientid].RemoteEntpoint);

					switch (requestPacket.BootpOPCode)
					{
						case BOOTPOPCode.BootRequest:
							Thread.Sleep(10);
							switch ((DHCPMessageType)requestPacket.GetOption((byte)DHCPOptions.DHCPMessageType).Data[0])
							{
								
								case DHCPMessageType.Discover:
									Clients[clientid].RemoteEntpoint.Address = IPAddress.Broadcast;
									Handle_DHCP_Discover(e.ServerId, e.SocketId, clientid, requestPacket);
									break;
								case DHCPMessageType.Request:
									Handle_DHCP_Request(e.ServerId, e.SocketId, clientid, requestPacket);
									break;
								case DHCPMessageType.Inform:
									break;
								case DHCPMessageType.Release:
									if (Clients.ContainsKey(clientid))
										Clients.Remove(clientid);
									break;
								default:
									break;
							}
							break;
						case BOOTPOPCode.BootReply:
							break;
						default:
							break;
					}
					break;
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
				Ports.AddRange(from port in ports
					select ushort.Parse(port.Trim()));
			}

			bootServers.Add(new BootServer(Environment.MachineName));

			AddServer?.Invoke(this, new(ServiceType, Ports));
			return true;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}


		private void Handle_RBCP_Request(string client, DHCPPacket request)
		{
			var venEncOpts = request.GetEncOptions(43);
			foreach (var option in venEncOpts)
			{
				switch ((PXEVendorEncOptions)option.Option)
				{
					case PXEVendorEncOptions.MultiCastIPAddress:
						break;
					case PXEVendorEncOptions.MulticastClientPort:
						break;
					case PXEVendorEncOptions.MulticastServerPort:
						break;
					case PXEVendorEncOptions.MulticastTFTPTimeout:
						break;
					case PXEVendorEncOptions.MulticastTFTPDelay:
						break;
					case PXEVendorEncOptions.DiscoveryControl:
						break;
					case PXEVendorEncOptions.DiscoveryMulticastAddress:
						break;
					case PXEVendorEncOptions.BootServers:
						break;
					case PXEVendorEncOptions.BootMenue:
						break;
					case PXEVendorEncOptions.MenuPrompt:
						break;
					case PXEVendorEncOptions.MulticastAddressAllocation:
						break;
					case PXEVendorEncOptions.CredentialTypes:
						break;
					case PXEVendorEncOptions.BootItem:
						var itemType = new byte[sizeof(ushort)];
						Array.Copy(option.Data, 0, itemType, 0, itemType.Length);
						Clients[client].RBCP.Item = BinaryPrimitives.ReadUInt16BigEndian(itemType);

						var itemLayer = new byte[sizeof(ushort)];
						Array.Copy(option.Data,2, itemLayer, 0, itemLayer.Length);
						Clients[client].RBCP.Layer = BinaryPrimitives.ReadUInt16BigEndian(itemLayer);

						Console.WriteLine("PXE Item: {0}", Clients[client].RBCP.Item);
						switch (Clients[client].RBCP.Layer)
						{
							case 0:
								Console.WriteLine("[RBCP] Layer: Client Bootfile request...");
								break;
							case 1:
								Console.WriteLine("[RBCP] Layer: Client Credential request...");
								break;
							default:
								break;
						}
						break;
					case PXEVendorEncOptions.End:
						break;
					default:
						break;
				}
			}
		}

		private void Handle_DHCP_Discover(Guid server, Guid socket, string client, DHCPPacket packet)
		{
			var serverIP = NetbootBase.Servers[server].Get_IPAddress(socket);
			var response = packet.CreateResponse(serverIP);

			response.FileName = "Boot/x86/apitest.0";

			Handle_RBCP_Request(client, packet);
			var vendorOptions = new List<DHCPOption>
			{
				Functions.GenerateBootServersList(bootServers),
				Functions.GenerateBootMenue(bootServers),
				Functions.GenerateBootMenuePrompt(),
				new DHCPOption(6,3)
			};

			response.AddOption(new DHCPOption(43, vendorOptions));

			response.CommitOptions();
			ServerSendPacket.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));
		}



		private void Handle_DHCP_Request(Guid server, Guid socket, string client, DHCPPacket packet)
		{
			#region "Ensure that we respond only to DHCP requests which are for us!"
			var serverIP = NetbootBase.Servers[server].Get_IPAddress(socket);
			
			if (packet.Options.ContainsKey((byte)DHCPOptions.ServerIdentifier))
			{
				var serverIdent = new IPAddress(packet.Options[(byte)DHCPOptions.ServerIdentifier].Data);
				if (!serverIP.Equals(serverIdent))
					return;
			}
			#endregion

			var response = packet.CreateResponse(serverIP);
				Handle_RBCP_Request(client, packet);

			var layer = 0;

			var options = response.GetEncOptions(43);
			foreach (var option in options)
			{
				if (option.Option == 71)
				{
					var layerBytes = new byte[sizeof(ushort)];
					Array.Copy(option.Data, 2, layerBytes, 0, layerBytes.Length);
					layer = BinaryPrimitives.ReadUInt16BigEndian(layerBytes);
					break;
				}
			}

			response.FileName = "Boot/x86/apitest.0";

			response.CommitOptions();
			ServerSendPacket.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));

			if (Clients.ContainsKey(client))
				Clients.Remove(client);
		}

		public void Heartbeat()
		{
		}
	}
}
