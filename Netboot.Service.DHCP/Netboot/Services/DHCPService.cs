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
using Netboot.Network.Packet;
using Netboot.Network.Sockets;
using Netboot.Services.Interfaces;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
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
					bootServers.Add(hostname, new(hostname, BootServerType));
				else
					value.Type = e.BootServerType;

				MenueTimeout = e.MenueTimeout;
				RespondDelay = e.RespondDelay;

				Console.WriteLine($"Bootserver behavior set to: {BootServerType}");
			};
			#endregion

			UpdateBootfile += (sender, e) =>
			{
				Console.WriteLine("Bootfile changed to: {0}", e.Bootfile);
				Clients[e.Client].Response.FileName = e.Bootfile;
			};
		}

		Dictionary<string, BootServer> bootServers = [];

		Dictionary<BootServerTypes, string> bootfiles = [];

		public SocketProtocol Protocol { get; set; } = SocketProtocol.UDP;

		public List<ushort> Ports { get; set; } = [];

		public string ServiceType { get; }

		private IPAddress McastDiscoveryAddress { get; set; } = IPAddress.Parse("224.0.1.2");

		private ushort McastClientPort { get; set; } = 4001;

		private ushort McastServerPort { get; set; } = 69;

		private byte DiscoveryControl { get; set; } = 3;

		private ushort MulticastTimeout { get; set; } = 10;

		private ushort MulticastDelay { get; set; } = 10;

		private ServerMode ServerMode { get; set; } = ServerMode.AllowAll;

		public Dictionary<BootServerTypes, Dictionary<DHCPOptions, byte[]>> ServiceDHCPOptions { get; private set; } = [];

		public BootServerTypes BootServerType { get; private set; }

		public byte MenueTimeout { get; private set; } = byte.MaxValue;

		/// <summary>
		/// We may respond too quickly, so we should respond with x milliseconds delay,
		/// so that the DHCP server can respond before us.
		/// </summary>
		public byte RespondDelay { get; private set; } = 10;

		public Dictionary<string, DHCPClient> Clients { get; set; } = [];

		public event AddServerEventHandler? AddServer;
		public event ServerSendPacketEventHandler? ServerSendPacket;
		public event PrintMessageEventHandler? PrintMessage;

		public void Dispose()
		{
			foreach (var client in Clients.Values.ToList())
				client.Dispose();

			Clients.Clear();
			Ports.Clear();
		}

		void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId, DHCPVendorID vendorID)
		{
			var client = new DHCPClient(false, clientId, serviceType, remoteEndpoint, serverId, socketId, vendorID);

			if (!Clients.TryAdd(clientId, client))
			{
				Clients[clientId] = client;

				if (Clients[clientId].RemoteEndpoint.Address.Equals(IPAddress.Any))
					Clients[clientId].RemoteEndpoint.Address = IPAddress.Broadcast;
			}

			Clients[clientId].UpdateTimestamp();
		}

		private void RemoveClient(string id)
		{
			if (Clients.ContainsKey(id))
			{
				Clients[id].Dispose();
				Clients.Remove(id);
			}
		}

		public void Handle_WDS_Request(string client, DHCPPacket request)
		{
			var wdsData = request.GetEncOptions(250);
			foreach (var wdsOption in wdsData)
			{
				switch ((WDSNBPOptions)wdsOption.Option)
				{
					case WDSNBPOptions.Unknown:
						break;
					case WDSNBPOptions.Architecture:
						Clients[client].Architecture = (Architecture)wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.NextAction:
						Clients[client].WDS.NextAction = (NextActionOptionValues)wdsOption.AsByte();
						break;
					case WDSNBPOptions.PollInterval:
						Clients[client].WDS.PollInterval = wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.PollRetryCount:
						Clients[client].WDS.RetryCount = wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.RequestID:
						Clients[client].WDS.RequestId = wdsOption.AsUInt32();
						break;
					case WDSNBPOptions.VersionQuery:
						Clients[client].WDS.VersionQuery = true;
						break;
					case WDSNBPOptions.ServerVersion:
						Clients[client].WDS.ServerVersion = (NBPVersionValues)wdsOption.AsUInt32();
						break;
					case WDSNBPOptions.ReferralServer:
						Clients[client].WDS.ReferralServer = wdsOption.AsIPAddress();
						break;
					case WDSNBPOptions.PXEClientPrompt:
						Clients[client].WDS.ClientPrompt = (PXEPromptOptionValues)wdsOption.AsByte();
						break;
					case WDSNBPOptions.PxePromptDone:
						Clients[client].WDS.PromptDone = (PXEPromptOptionValues)wdsOption.AsByte();
						break;
					case WDSNBPOptions.NBPVersion:
						Clients[client].WDS.NBPVersion = (NBPVersionValues)wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.ServerFeatures:
						Clients[client].WDS.ServerFeatures = wdsOption.AsUInt32();
						break;
					case WDSNBPOptions.ActionDone:
						Clients[client].WDS.ActionDone = wdsOption.AsBool();

						Clients[client].WDS.ActionDone = (ServerMode == ServerMode.AllowAll);
						break;
					default:
						break;
				}
			}
		}

		public DHCPOption Handle_WDS_Options(string client, DHCPPacket request)
		{
			var options = new List<DHCPOption>
			{
				new((byte)WDSNBPOptions.NextAction, (byte)Clients[client].WDS.NextAction),
				new ((byte)WDSNBPOptions.PXEClientPrompt, (byte)Clients[client].WDS.PromptDone),
				new ((byte)WDSNBPOptions.ActionDone, Convert.ToByte(Clients[client].WDS.ActionDone)),
				new ((byte)WDSNBPOptions.PollRetryCount, Clients[client].WDS.RetryCount),
			};

			var requestIDBytes = new byte[sizeof(uint)];
			BinaryPrimitives.WriteUInt32BigEndian(requestIDBytes, (uint)Clients.Count);
			options.Add(new((byte)WDSNBPOptions.RequestID, requestIDBytes));

			var polldelayBytes = new byte[sizeof(short)];
			BinaryPrimitives.WriteUInt16BigEndian(polldelayBytes, Clients[client].WDS.PollInterval);
			options.Add(new((byte)WDSNBPOptions.PollInterval, polldelayBytes));

			options.Add(new((byte)WDSNBPOptions.PXEClientPrompt, (byte)Clients[client].WDS.ClientPrompt));
			options.Add(new((byte)WDSNBPOptions.AllowServerSelection, Convert.ToByte(Clients[client].WDS.ServerSelection)));

			switch (Clients[client].WDS.NextAction)
			{
				case NextActionOptionValues.Approval:
					options.Add(new((byte)WDSNBPOptions.Message, Clients[client].WDS.AdminMessage, Encoding.ASCII));
					break;
				case NextActionOptionValues.Referral:
					options.Add(new((byte)WDSNBPOptions.ReferralServer, Clients[client].WDS.ReferralServer));
					break;
				default:
					break;
			}

			return new(250, options);
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			var requestPacket = new DHCPPacket(e.ServiceType, e.Packet);
			Thread.Sleep(RespondDelay);

			var clientid = string.Join(":", requestPacket.HardwareAddress.Select(x => x.ToString("X2")));
			switch (requestPacket.GetVendorIdent)
			{
				case DHCPVendorID.PXEClient:
				case DHCPVendorID.PXEServer:
					AddClient(clientid, e.ServiceType, e.RemoteEndpoint, e.ServerId, e.SocketId, requestPacket.GetVendorIdent);

					Console.WriteLine("[I] Got {1}Request from: {0}", Clients[clientid].RemoteEndpoint,
						!requestPacket.GatewayIP.Equals(IPAddress.Any) ? "relayed " : string.Empty);

					switch (requestPacket.BootpOPCode)
					{
						case BOOTPOPCode.BootRequest:

							if (!requestPacket.GatewayIP.Equals(IPAddress.Any))
								Clients[clientid].RemoteEndpoint.Address = requestPacket.GatewayIP;

							switch ((DHCPMessageType)requestPacket.GetOption((byte)DHCPOptions.DHCPMessageType).Data[0])
							{
								case DHCPMessageType.Discover:
									Clients[clientid].RemoteEndpoint.Address = IPAddress.Broadcast;
									Handle_DHCP_Discover(e.ServerId, e.SocketId, clientid, requestPacket);
									RemoveClient(clientid);
									break;
								case DHCPMessageType.Inform:
								case DHCPMessageType.Request:
									Handle_DHCP_Request(e.ServerId, e.SocketId, clientid, requestPacket);
									break;
								case DHCPMessageType.Release:
								default:
									RemoveClient(clientid);
									break;
							}
							break;
						case BOOTPOPCode.BootReply:
						default:
							RemoveClient(clientid);
							break;
					}
					break;
				default:
					RemoveClient(clientid);
					Console.WriteLine("[W] DHCP: Vendor ID not supported.");
					return;
			}
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
			=> Console.WriteLine(e.BytesSent);

		public bool Initialize(XmlNode xmlConfigNode)
		{
			if (xmlConfigNode == null)
				return false;

			var ports = xmlConfigNode.Attributes.GetNamedItem("port").Value.Split(',').ToList();
			if (ports.Count > 0)
				Ports.AddRange(from port in ports
							   select ushort.Parse(port.Trim()));

			var bserv_val = xmlConfigNode.Attributes.GetNamedItem("behavior").Value;

			if (string.IsNullOrEmpty(bserv_val))
				return false;

			var bservType = (BootServerTypes)ushort.Parse(bserv_val);

			var menueTimeout_val = xmlConfigNode.Attributes.GetNamedItem("timeout").Value;

			if (string.IsNullOrEmpty(menueTimeout_val))
				return false;

			var menueTimeout = byte.Parse(menueTimeout_val);

			var respondDelay_val = xmlConfigNode.Attributes.GetNamedItem("delay").Value;

			if (string.IsNullOrEmpty(respondDelay_val))
				return false;

			var respondDelay = byte.Parse(respondDelay_val);

			DHCPServiceBehavior.Invoke(this, new(bservType, menueTimeout, respondDelay));

			#region "Read Multicast settings"
			var mcastSettings = xmlConfigNode.SelectNodes("Multicast");

			if (mcastSettings.Count != 0)
			{
				foreach (XmlNode mcastSetting in mcastSettings)
				{
					McastDiscoveryAddress = IPAddress.Parse(mcastSetting.Attributes.GetNamedItem("addr").Value);
					McastClientPort = ushort.Parse(mcastSetting.Attributes.GetNamedItem("cport").Value);
					McastServerPort = ushort.Parse(mcastSetting.Attributes.GetNamedItem("sport").Value);

					MulticastDelay = ushort.Parse(mcastSetting.Attributes.GetNamedItem("startdelay").Value);
					MulticastTimeout = ushort.Parse(mcastSetting.Attributes.GetNamedItem("timeout").Value);
					DiscoveryControl = byte.Parse(mcastSetting.Attributes.GetNamedItem("discovery").Value);
				}
			}

			#endregion

			#region "Bootfiles from Config"

			XmlNodeList? bfiles = xmlConfigNode.SelectNodes("Bootfiles/Bootfile");
			foreach (XmlNode bootfile in bfiles)
			{
				var val = bootfile.Attributes.GetNamedItem("behavior").Value;
				if (string.IsNullOrEmpty(val))
					continue;

				var behavior = (BootServerTypes)ushort.Parse(val);
				var file = bootfile.InnerText;

				bootfiles.Add(behavior, file);
			}
			#endregion

			#region "Read behavior based DHCP Options"
			var dhcpList = xmlConfigNode.SelectNodes("DHCP");
			foreach (XmlNode dhcp in dhcpList)
			{
				var behavior = (BootServerTypes)ushort.Parse(dhcp.Attributes.GetNamedItem("behavior").Value);
				if (!ServiceDHCPOptions.ContainsKey(behavior))
					ServiceDHCPOptions.Add(behavior, new Dictionary<DHCPOptions, byte[]>());

				var optionList = dhcp.SelectNodes("Option");
				foreach (XmlNode option in optionList)
				{
					var opt = (DHCPOptions)byte.Parse(option.Attributes.GetNamedItem("id").Value);
					var dataTypeRaw = option.Attributes.GetNamedItem("type").Value;

					switch (dataTypeRaw)
					{
						case "string":
							ServiceDHCPOptions[behavior].Add(opt, Encoding.ASCII.GetBytes(option.InnerText));
							break;
						case "uint8":
							var x = new byte[1] { byte.Parse(option.Value) };
							ServiceDHCPOptions[behavior].Add(opt, x);
							break;
						case "uint16":
							ServiceDHCPOptions[behavior].Add(opt, BitConverter.GetBytes(ushort.Parse(option.InnerText)));
							break;
						case "uint32":
							ServiceDHCPOptions[behavior].Add(opt, BitConverter.GetBytes(uint.Parse(option.InnerText)));
							break;
						case "ipaddr":
							ServiceDHCPOptions[behavior].Add(opt, IPAddress.Parse(option.InnerText).GetAddressBytes());
							break;
						default:
							break;
					}
				}
			}
			#endregion

			AddServer?.Invoke(this, new(ServiceType, Protocol, Ports));

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
					case PXEVendorEncOptions.BootItem:
						var itemType = new byte[sizeof(ushort)];
						Array.Copy(option.Data, 0, itemType, 0, itemType.Length);
						Clients[client].RBCP.Item = BinaryPrimitives.ReadUInt16BigEndian(itemType);

						var itemLayer = new byte[sizeof(ushort)];
						Array.Copy(option.Data, sizeof(ushort), itemLayer, 0, itemLayer.Length);
						Clients[client].RBCP.Layer = BinaryPrimitives.ReadUInt16BigEndian(itemLayer);
						break;
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


			switch (Clients[client].VendorID)
			{
				case DHCPVendorID.HTTPClient:
				case DHCPVendorID.PXEClient:
				case DHCPVendorID.PXEServer:
					#region "Remote Boot Configuration Protocol (RBCP)"
					Handle_RBCP_Request(client, packet);

					Clients[client].Response.AddOption(new((byte)DHCPOptions.VendorSpecificInformation,
						new List<DHCPOption> {
				Functions.GenerateBootMenuePrompt(MenueTimeout),
				Functions.GenerateBootServersList(bootServers),
				Functions.GenerateBootMenue(bootServers),

				new((byte)PXEVendorEncOptions.DiscoveryControl, DiscoveryControl),
					}));
					#endregion

					#region "Windows Deployment Server"
					if (BootServerType == BootServerTypes.WindowsDeploymentServer)
						Clients[client].Response.AddOption(Handle_WDS_Options(client, packet));
					#endregion
					break;
				case DHCPVendorID.AAPLBSDPC:
					break;
				default:
					Console.WriteLine("[!] DHCP Request not supported!");
					break;
			}

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

			var bootfile = GetBootfile(client);

			switch (Clients[client].VendorID)
			{
				case DHCPVendorID.HTTPClient:
				case DHCPVendorID.PXEClient:
				case DHCPVendorID.PXEServer:
					Handle_RBCP_Request(client, packet);
					#region "Windows Deployment Server"
					if (BootServerType == BootServerTypes.WindowsDeploymentServer && packet.HasOption(250))
					{
						Handle_WDS_Request(client, packet);
						bootfile = GetBootfile(client, Clients[client].WDS.ActionDone
							? "Boot\\#arch#\\pxeboot.n12" : "Boot\\#arch#\\wdsnbp.com");

						Clients[client].Response.AddOption(new DHCPOption(252, "Boot\\#arch#\\default.bcd"
						.Replace("#arch#", GetArchitecture(Clients[client].Architecture)), Encoding.ASCII));
					}

					if (BootServerType == BootServerTypes.WindowsDeploymentServer && packet.HasOption(250) && !Clients[client].WDS.ActionDone)
						return;

					if (BootServerType == BootServerTypes.WindowsDeploymentServer)
						Clients[client].Response.AddOption(Handle_WDS_Options(client, packet));

					if (ServerMode == ServerMode.KnownOnly)
					{
						Clients[client].Response.AddOption(new((byte)DHCPOptions.Message, $" Pending Request ID:" +
							$" {Clients.Count}: {Clients[client].WDS.AdminMessage}", Encoding.ASCII));
					}
					#endregion
					break;
				case DHCPVendorID.AAPLBSDPC:
					break;
				default:
					break;
			}

			if (BootServerType != BootServerTypes.Apple)
				UpdateBootfile?.Invoke(server, new(bootfile, Clients[client].RBCP.Layer, client));

			#region "Add Behavior specific DHCP Options"
			if (ServiceDHCPOptions.TryGetValue(BootServerType, out var options))
				foreach (var option in options)
					Clients[client].Response.AddOption(new((byte)option.Key,
						option.Value));
			#endregion

			Clients[client].Response.CommitOptions();

			try
			{
				ServerSendPacket?.Invoke(this, new(ServiceType, server, socket,
					Clients[client].Response, Clients[client]));
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"{Clients[client].RemoteEndpoint}: {ex.Message}");
			}

			RemoveClient(client);
		}

		private string GetArchitecture(Architecture architecture)
		{
			return architecture switch
			{
				Architecture.X86PC => "x86",
				Architecture.EFI_x8664 => "x64",
				Architecture.EFIItanium => "ia32",
				Architecture.DECAlpha => "alpha",
				Architecture.Arcx86 => "x86",
				Architecture.EFIByteCode => "efi",
				Architecture.EFI_IA32 => "x64",
				_ => "other"
			};
		}

		private string GetBootfile(string client, string filename = "")
		{
			var file = string.IsNullOrEmpty(filename) ? bootfiles[BootServerType] : filename;

			return file.Replace("#layer#", string.Format("{0}", Clients[client].RBCP.Layer))
				.Replace("#arch#", GetArchitecture(Clients[client].Architecture));
		}

		public void Heartbeat(DateTime now)
		{
			foreach (var client in Clients.ToList())
			{
				var ts = now - client.Value.LastUpdate;

				if (ts.TotalSeconds >= 30)
					RemoveClient(client.Key);
			}
		}
	}
}
