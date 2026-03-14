using Netboot.Common;
using Netboot.Module.DHCPListener;
using Netboot.Module.DHCPListener.Interfaces;
using System.Net;
using System.Text;

namespace DHCPListener.BSvcMod.RBCP
{
	public class PxeRBCP : IBootService, IDHCPListener
    {
		Dictionary<Guid, IRBCPClient> Clients = [];

		private Dictionary<string, BootServer> bootServers = [];

		public BootServerType ServerType { get; set; } = BootServerType.PXEBootstrapServer;

		public PxeRBCP()
		{
			DHCPListenerBase.BootServiceRequest += (sender, e) =>
			{
				Handle_Listener_Request(e.Server, e.Socket, e.Client, e.Request);
			};

			var hostname = Environment.MachineName;
			bootServers.Add(Guid.Empty.ToString(), new BootServer(hostname, ServerType));
			DHCPListenerBase.RegisterBootService(this, ServerType, hostname);


			//Clients.Add(Guid.Empty, new RBCPClient(true, Guid.Empty,))
		}

		public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
		{
			var requestPacket = new DHCPPacket(memoryStream);

			if (requestPacket.HasOption(DHCPOptions.ServerIdentifier))
			{
				var ipPrefServer = requestPacket.GetOption((byte)DHCPOptions.ServerIdentifier).AsIPAddress();

				if (!NetbootBase.NetworkManager.ServerManager.Servers[server]
					.GetEndPoint(socket).Address.Equals(ipPrefServer))
					return;
			}

            switch (requestPacket.BootpOPCode)
			{
				case BOOTPOPCode.BootRequest:

                    switch (requestPacket.GetVendorIdent)
					{
						case DHCPVendorID.PXEClient:
							if (requestPacket.GetMessageType() != DHCPMessageType.Discover)
								return;

                            #region Get the UUID (GUID) of the Client and add him
                            var clientId = requestPacket.HardwareAddress.ToGuid();
                            Clients[clientId] = new RBCPClient(false, clientId, requestPacket, server, socket, client);

                            var idBytes = new byte[Guid.NewGuid().ToByteArray().Length];

							var opt = requestPacket.GetOption((byte)DHCPOptions.UuidGuidBasedClientIdentifier).AsByte();
							switch ((ClientIdentType)opt)
							{
								case ClientIdentType.UUID:
									idBytes[0] = opt;
									clientId = Netboot.Module.DHCPListener.Functions.AsLittleEndianGuid(idBytes);

                                    NetbootBase.Log("D", this.GetType().ToString(), string.Format("%s", clientId.ToString()));
                                    break;
								default:
									break;
							}

							if (requestPacket.HasOption(43))
							{
								var enCapOpts = requestPacket.GetEncOptions(43);
								if (enCapOpts.ContainsKey(71))
								{
									var bsType = enCapOpts[71].AsUInt16();
									NetbootBase.Log("D", this.GetType().ToString(), string.Format("%S", bsType));
									return;
								}
							}



                            var serverIP = NetbootBase.NetworkManager.ServerManager.GetEndPoint(server, socket);
							Clients[clientId].Response = Clients[clientId].Request.CreateResponse(serverIP.Address);

							#endregion

							Handle_BootService_Request(clientId, Clients[clientId].Request);
							break;
						default:
                            NetbootBase.Log("I", string.Format("DHCP[{0}]", ServerType),
								string.Format("Got {0} ({1}) request from Client: {2}", requestPacket.GetMessageType(),
								requestPacket.GetVendorIdent, client));
                            return;
					}

					break;
				case BOOTPOPCode.BootReply:

					var srvIP = Clients[client].Request.Options[(byte)DHCPOptions.ServerIdentifier].AsIPAddress();

                    NetbootBase.Log("I", string.Format("DHCP[{0}]", ServerType),
						string.Format("Received {0} reply from DHCP Server: {1} ({2})", requestPacket.GetMessageType(), client, srvIP));
                    break;
				default:
					return;
			}
		}

		public void Handle_BootService_Request(string client, DHCPPacket requestPacket)
			=> Handle_BootService_Request(Guid.Parse(client), requestPacket);

		public void Handle_BootService_Request(Guid client, DHCPPacket requestPacket)
		{
			NetbootBase.Log("I", string.Format("DHCP[{0}]", ServerType),
				string.Format("Got {0} request from Client: {1}", requestPacket.GetMessageType(),
					client));

			switch (requestPacket.GetMessageType())
			{
				case DHCPMessageType.Discover:
					Handle_DHCP_Discover(client, requestPacket);
					break;
				case DHCPMessageType.Request:
					Handle_DHCP_Request(client, requestPacket);
					break;
				default:
					return;
			}
		}

		public void Handle_DHCP_Discover(Guid clientid, DHCPPacket request)
		{
			Clients[clientid].Response.Flags = BootpFlags.Broadcast;

			var EncVendorOptions = new List<DHCPOption<PXEVendorEncOptions>>
			{
				new(PXEVendorEncOptions.MulticastTFTPDelay,Clients[clientid].MulticastDelay),
				new(PXEVendorEncOptions.DiscoveryMulticastAddress, Clients[clientid].McastDiscoveryAddress),
				GenerateBootServersList(bootServers),
				GenerateBootMenuePrompt(),
				GenerateBootMenue(bootServers),
				new(PXEVendorEncOptions.DiscoveryControl,
					Clients[clientid].DiscoveryControl),
				new(PXEVendorEncOptions.End)
			};


			Clients[clientid].Response.AddOption(new DHCPOption<byte>((byte)DHCPOptions.VendorClassIdentifier, "PXEClient", Encoding.ASCII));
			Clients[clientid].Response.AddOption(new DHCPOption<byte>((byte)DHCPOptions.VendorSpecificInformation, EncVendorOptions));

			var bytes = Clients[clientid].Response.Buffer.GetBuffer();
			Clients[clientid].Response.CommitOptions();

			var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientid].Server, Clients[clientid].Socket, Clients[clientid].Client);
			if (endpoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
				endpoint.Address = IPAddress.Broadcast;

			NetbootBase.NetworkManager.ServerManager.Servers[Clients[clientid].Server].Send(Clients[clientid].Socket, Clients[clientid].Client,
				endpoint, bytes);
		}

		public void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
		{
		}

		public static DHCPOption<PXEVendorEncOptions> GenerateBootServersList(Dictionary<string, BootServer> serverlist)
		{
			var serverlistBlock = new byte[byte.MaxValue];
			var sbIndex = 0;

			foreach (var server in serverlist.Values.ToList())
			{
				var serverbytes = server.AsBytes();
				var srvLength = serverbytes.Length;

				Array.Copy(serverbytes, 0, serverlistBlock, sbIndex, srvLength);

				sbIndex += srvLength;
			}

			Array.Resize(ref serverlistBlock, sbIndex);

			return new DHCPOption<PXEVendorEncOptions>(PXEVendorEncOptions.BootServer, serverlistBlock);
		}

		public static DHCPOption<PXEVendorEncOptions> GenerateBootMenue(Dictionary<string, BootServer> servers)
		{
			#region Setup the Menue itself...
			var menubuffer = new byte[byte.MaxValue];
			var mbIndex = 0;

			var bootmenue = new List<BootMenueEntry>
			{
				new(BootServerType.PXEBootstrapServer, "Local Boot")
			};

			foreach (var server in servers.ToList())
			{
				if (server.Value.Addresses.Count == 0 || string.IsNullOrEmpty(server.Value.Hostname))
					continue;

				var bsType = server.Value.Type;

				if (bsType == BootServerType.PXEBootstrapServer)
					continue;

				bootmenue.Add(new(bsType, string.Format("[{0}] {1}", server.Value.Hostname, bsType)));
			}

			#endregion

			foreach (var entry in bootmenue.ToList())
			{
				var entryBytes = entry.AsBytes();
				var menuLength = entryBytes.Length;

				Array.Copy(entryBytes, 0, menubuffer, mbIndex, menuLength);

				mbIndex += menuLength;
			}

			Array.Resize(ref menubuffer, mbIndex);
			return new DHCPOption<PXEVendorEncOptions>(PXEVendorEncOptions.BootMenue, menubuffer);
		}

		public void HeartBeat()
		{
			bootServers.Clear();
			bootServers.Add(Guid.Empty.ToString(), new(Environment.MachineName, ServerType));

			foreach (var key in DHCPListenerBase.Bootservices.Keys.ToList())
			{
				if (key == BootServerType.PXEBootstrapServer)
					continue;

				bootServers.Add(Guid.NewGuid().ToString(), new(Environment.MachineName, key));
			}

			NetbootBase.Log("I", "DHCP", "Bootservers updated!");
		}

		public static DHCPOption<PXEVendorEncOptions> GenerateBootMenuePrompt(byte timeout = byte.MaxValue)
		{
			var prompt = Encoding.ASCII.GetBytes(timeout == byte.MaxValue ? "Select Server..." :
				"Press [F8] to boot from Network or [esc] to cancel...");

			var promptbuffer = new byte[1 + prompt.Length];
			var offset = 0;

			#region "Timeout"
			promptbuffer[offset] = timeout;
			offset += sizeof(byte);
			#endregion

			#region "Prompt"
			Array.Copy(prompt, 0, promptbuffer, offset, prompt.Length);
			#endregion

			return new DHCPOption<PXEVendorEncOptions>(PXEVendorEncOptions.MenuPrompt, promptbuffer);
		}
    }
}
