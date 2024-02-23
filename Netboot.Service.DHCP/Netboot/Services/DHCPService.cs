/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using Netboot.Services.Interfaces;
using System.Buffers.Binary;
using System.Net;
using System.Text;
using System.Xml;
using static Netboot.Services.Interfaces.IService;

namespace Netboot.Services.DHCP
{
	public class DHCPService : IService
	{
		public delegate void DHCPServiceBehaviorEventHandler(object sender, DHCPServiceBehaviorEventargs e);
		public event DHCPServiceBehaviorEventHandler DHCPServiceBehavior;

		public delegate void UpdateBootfileEventHandler(object sender, UpdateBootfileEventargs e);
		public event UpdateBootfileEventHandler UpdateBootfile;

		public DHCPService(string serviceType)
		{
			ServiceType = serviceType;

			#region "ServiceBehavior"
			DHCPServiceBehavior += (sender, e) =>
			{
				BootServerType = e.BootServerType;

				var hostname = Environment.MachineName;

				if (!bootServers.TryGetValue(hostname, out BootServer? value))
					bootServers.Add(hostname, new (hostname, BootServerType));
				else
					value.Type = e.BootServerType;

				MenueTimeout = e.MenueTimeout;
				RespondDelay = e.RespondDelay;

				Console.WriteLine($"Bootserver behavior set to: {BootServerType}");
			};
			#endregion

			UpdateBootfile += (sender, e) => {
				Console.WriteLine("Bootfile changes to: {0}", e.Bootfile);
				Clients[e.Client].Response.FileName = e.Bootfile;
			};
		}

		Dictionary<string, BootServer> bootServers = [];

		public List<ushort> Ports { get; set; } = [];

		public string ServiceType { get; }

		public BootServerTypes BootServerType { get; private set; }

		public byte MenueTimeout { get; private set; } = byte.MaxValue;

		public byte RespondDelay { get; private set; } = 1;

		public Dictionary<string, DHCPClient> Clients { get; set; } = [];

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
			var client = new DHCPClient(clientId, serviceType, remoteEndpoint, serverId, socketId);

			if (!Clients.TryAdd(clientId, client))
				Clients[clientId] = client;
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			var requestPacket = new DHCPPacket(e.ServiceType, e.Packet);
			
			Thread.Sleep(RespondDelay);

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
							switch ((DHCPMessageType)requestPacket.GetOption((byte)DHCPOptions.DHCPMessageType).Data[0])
							{
								case DHCPMessageType.Discover:
									Clients[clientid].RemoteEntpoint.Address = IPAddress.Broadcast;
									Handle_DHCP_Discover(e.ServerId, e.SocketId, clientid, requestPacket);
									break;
								case DHCPMessageType.Request:
									Handle_DHCP_Request(e.ServerId, e.SocketId, clientid, requestPacket);
									if (Clients.ContainsKey(clientid))
										Clients.Remove(clientid);
									break;
								case DHCPMessageType.Inform:
									break;
								case DHCPMessageType.Release:
								default:
									if (Clients.ContainsKey(clientid))
										Clients.Remove(clientid);
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
				Ports.AddRange(from port in ports
					select ushort.Parse(port.Trim()));

			var bservType = (BootServerTypes)ushort.Parse(xmlConfigNode.Attributes.GetNamedItem("behavior").Value);
			var menueTimeout = byte.Parse(xmlConfigNode.Attributes.GetNamedItem("timeout").Value);
			var respondDelay = byte.Parse(xmlConfigNode.Attributes.GetNamedItem("delay").Value);

			DHCPServiceBehavior.Invoke(this, new(bservType, menueTimeout, respondDelay));

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
			var venEncOpts = request.GetEncOptions((byte)DHCPOptions.VendorSpecificInformation);
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
					case PXEVendorEncOptions.BootServer:
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
						Array.Copy(option.Data, 2, itemLayer, 0, itemLayer.Length);
						Clients[client].RBCP.Layer = BinaryPrimitives.ReadUInt16BigEndian(itemLayer);

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
					default:
						break;
				}
			}
		}

		private void Handle_DHCP_Discover(Guid server, Guid socket, string client, DHCPPacket packet)
		{
			var serverIP = NetbootBase.Servers[server].Get_IPAddress(socket);
			Clients[client].Response = packet.CreateResponse(serverIP);
			UpdateBootfile?.Invoke(server, new(GetBootfile(client), 0, client));

			Handle_RBCP_Request(client, packet);


			var vendorOptions = new List<DHCPOption>
			{
				new DHCPOption(1, IPAddress.Parse("224.0.1.2")),
				Functions.GenerateBootMenuePrompt(MenueTimeout),
				Functions.GenerateBootServersList(bootServers),
				Functions.GenerateBootMenue(bootServers),

				new((byte)PXEVendorEncOptions.DiscoveryControl, (byte)3)
			};

			Clients[client].Response.AddOption(new(43, vendorOptions));

			Clients[client].Response.CommitOptions();
			ServerSendPacket.Invoke(this, new(ServiceType, server, socket, Clients[client].Response, Clients[client]));
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

			Clients[client].Response = packet.CreateResponse(serverIP);
			Handle_RBCP_Request(client, packet);

			UpdateBootfile?.Invoke(server, new(GetBootfile(client), 0, client));

			switch (BootServerType)
			{
				case BootServerTypes.MicrosoftWindowsNT:
					Clients[client].Response.AddOption(new (251, "Boot/x86/ris/oschoice.exe", Encoding.ASCII));
					Clients[client].Response.AddOption(new (254, "Boot/x86/ris/boot.ini", Encoding.ASCII));
					break;
				case BootServerTypes.Linux:
					Clients[client].Response.AddOption(new (210, "/linux", Encoding.ASCII));
					break;
				default:
					break;
			}

			Clients[client].Response.CommitOptions();
			ServerSendPacket.Invoke(this, new(ServiceType, server, socket, Clients[client].Response, Clients[client]));

			if (Clients.ContainsKey(client))
				Clients.Remove(client);
		}

		private string GetBootfile(string client)
		{
			var bFile = string.Empty;
			var layer = Clients[client].RBCP.Layer;

			switch (BootServerType)
			{
				case BootServerTypes.PXEBootstrapServer:
					bFile = $"Boot/x86/bstrap.{layer}";
					break;
				case BootServerTypes.MicrosoftWindowsNT:
					bFile = "OSChooser\\i386\\startrom.n12";
					break;
				case BootServerTypes.IntelLCM:
					break;
				case BootServerTypes.DOSUNDI:
					bFile = $"Boot/x86/dosundi.{layer}";
					break;
				case BootServerTypes.NECESMPRO:
					break;
				case BootServerTypes.IBMWSoD:
					break;
				case BootServerTypes.IBMLCCM:
					break;
				case BootServerTypes.CAUnicenterTNG:
					break;
				case BootServerTypes.HPOpenView:
					break;
				case BootServerTypes.Reserved:
					break;
				case BootServerTypes.Vendor:
					break;
				case BootServerTypes.Linux:
					bFile = "Boot/x86/pxelinux.0";
					break;
				case BootServerTypes.BISConfig:
					bFile = "Boot/x86/bisconfig.0";
					break;
				case BootServerTypes.WindowsDeploymentServer:
					bFile = "Boot\\x86\\wdsnbp.com";
					break;
				case BootServerTypes.ApiTest:
					bFile = $"Boot/x86/apitest.{layer}";
					break;
				default:
					break;
			}

			return bFile;
		}

		public void Heartbeat()
		{
		}
	}
}
