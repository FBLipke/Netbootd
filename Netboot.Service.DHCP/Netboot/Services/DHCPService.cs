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

using Netboot.Common;
using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Packet;
using Netboot.Services.Interfaces;
using System.Buffers.Binary;
using System.Net;
using System.Text;
using System.Xml;
using YamlDotNet.Serialization;
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

		public BootServerTypes BootServerType { get; private set; }

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
				Ports.AddRange(from port in ports
							   select ushort.Parse(port.Trim()));

			BootServerType = (BootServerTypes)ushort.Parse(xmlConfigNode.Attributes.GetNamedItem("behavior").Value);

			Console.WriteLine($"Behavior set to: {BootServerType}");


			bootServers.Add(new BootServer(Environment.MachineName, BootServerType));

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
						Array.Copy(option.Data, 2, itemLayer, 0, itemLayer.Length);
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

			response.FileName = GetBootfile();

			Handle_RBCP_Request(client, packet);
			var vendorOptions = new List<DHCPOption>
			{Network.Definitions.Functions.GenerateBootMenuePrompt(),
				Network.Definitions.Functions.GenerateBootServersList(bootServers),
				Network.Definitions.Functions.GenerateBootMenue(bootServers),

				new DHCPOption(6,(byte)3)
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

			response.FileName = GetBootfile();

			switch (BootServerType)
			{
				case BootServerTypes.MicrosoftWindowsNT:
					response.AddOption(new DHCPOption(251, "Boot/x86/ris/oschoice.exe", Encoding.ASCII));
					response.AddOption(new DHCPOption(254, "Boot/x86/ris/boot.ini", Encoding.ASCII));
					break;
				case BootServerTypes.Linux:
					response.AddOption(new DHCPOption(210, "/linux", Encoding.ASCII));
					break;
				default:
					break;
			}

			response.CommitOptions();
			ServerSendPacket.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));

			if (Clients.ContainsKey(client))
				Clients.Remove(client);
		}

		private string GetBootfile()
		{
			var bFile = string.Empty;

			switch (BootServerType)
			{
				case BootServerTypes.PXEBootstrapServer:
					bFile = "Boot/x86/bstrap.0";
					break;
				case BootServerTypes.MicrosoftWindowsNT:
					bFile = "OSChooser\\i386\\startrom.n12";
					break;
				case BootServerTypes.IntelLCM:
					break;
				case BootServerTypes.DOSUNDI:
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
					bFile = "Boot/x86/bisconfig";
					break;
				case BootServerTypes.WindowsDeploymentServer:
					bFile = "Boot\\x86\\wdsnbp.com";
					break;
				case BootServerTypes.ApiTest:
					bFile = "Boot/x86/apitest.0";
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
