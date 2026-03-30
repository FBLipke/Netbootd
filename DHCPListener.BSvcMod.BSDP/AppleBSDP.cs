using Netboot.Common;
using Netboot.Common.Network.Interfaces;
using Netboot.Module.DHCPListener;
using Netboot.Module.DHCPListener.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace DHCPListener.BSvcMod.BSDP
{
	public class AppleBSDP: IBootService, IDHCPListener
    {
		Dictionary<Guid, IBSDPClient> Clients = [];

		public BootServerType ServerType { get; set; } = BootServerType.AppleBootServer;

		public AppleBSDP(XmlNode xml)
		{
            DHCPListenerBase.BootServiceRequest += (sender, e) =>
			{
				Handle_Listener_Request(e.Server, e.Socket, e.Client, e.Request);
			};

			var hostname = Environment.MachineName;
			DHCPListenerBase.RegisterBootService(this, ServerType, hostname);
		}

		public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
		{
			var requestPacket = new DHCPPacket(memoryStream);

			if (!NetbootBase.NetworkManager.ServerManager.HasSocket(server, socket))
			{
				NetbootBase.Log("E", GetType().ToString(), string.Format("Server {0} does not contains Socket {1}", server, socket));
				return;
			}

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
						case DHCPVendorID.AAPLBSDPC:
							NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
								string.Format("Got {0} ({1}) from Client: {1}", requestPacket.GetMessageType(),
								requestPacket.GetVendorIdent, client));
							#region Get the UUID (GUID) of the Client and add...
							var clientId = requestPacket.HardwareAddress.ToGuid();

							var idBytes = new byte[Guid.NewGuid().ToByteArray().Length];

							var opt = requestPacket.GetOption((byte)DHCPOptions.UuidGuidBasedClientIdentifier).AsByte();
							switch ((ClientIdentType)opt)
							{
								case ClientIdentType.UUID:
									idBytes[0] = opt;
									clientId = Netboot.Module.DHCPListener.Functions.AsLittleEndianGuid(idBytes);
                                    break;
								default:
									break;
							}
							
							Clients[clientId] = new BSDPClient(false, clientId, requestPacket, server, socket, client);

							if (requestPacket.HasOption(43))
							{
								var enCapOpts = requestPacket.GetEncOptions(43);
								if (enCapOpts.ContainsKey(71))
								{
									var bsType = enCapOpts[71].AsUInt16();
									return;
								}
							}

							var serverIP = NetbootBase.NetworkManager.ServerManager.GetEndPoint(server, socket);
							Clients[clientId].Response = Clients[clientId].Request.CreateResponse(serverIP.Address);

							#endregion

							Handle_BootService_Request(clientId, Clients[clientId].Request);
							break;
						default:
                            return;
					}

					break;
				case BOOTPOPCode.BootReply:
                    break;
				default:
					return;
			}
		}

		public void Handle_BootService_Request(string client, DHCPPacket requestPacket)
			=> Handle_BootService_Request(Guid.Parse(client), requestPacket);

		public void Handle_BootService_Request(Guid client, DHCPPacket requestPacket)
		{
			switch (requestPacket.GetMessageType())
			{
				case DHCPMessageType.Discover:
					//Handle_DHCP_Discover(client, requestPacket);
					break;
				case DHCPMessageType.Inform:
					Handle_DHCP_Inform(client, requestPacket);
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
			Clients[clientid].Response.AddOption(new((byte)DHCPOptions.VendorClassIdentifier, "AAPLBSDPC", Encoding.ASCII));

			var bytes = Clients[clientid].Response.Buffer.GetBuffer();
			Clients[clientid].Response.CommitOptions();

			var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientid].Server, Clients[clientid].Socket, Clients[clientid].Client);
			if (endpoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
				endpoint.Address = IPAddress.Broadcast;
		}

		public void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
		{
		}

		public void Handle_DHCP_Inform(Guid clientid, DHCPPacket request)
		{
			var encaps = request.GetEncOptions((byte)DHCPOptions.VendorSpecificInformation);
			
			foreach (var option in encaps)
			{
				switch ((BSDPVendorEncOptions)option.Value.Option)
				{
					case BSDPVendorEncOptions.MessageType:
						switch ((BSDPMsgType)option.Value.AsByte())
						{
							case BSDPMsgType.List:
								Handle_BSDP_List_Request(clientid, request);
								break;
							case BSDPMsgType.Select:
								Handle_BSDP_Select_Request(clientid, request);
								break;
							case BSDPMsgType.Failed:
								Handle_BSDP_Failed_Request(clientid, request);
								break;
							default:
								return;
						}
						break;
					default:
						break;
				}
			}
		}

		private void Handle_BSDP_Failed_Request(Guid clientid, DHCPPacket request)
		{
			NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
				string.Format("Got {0}[Failed] from Client: {1}", request.GetMessageType(), clientid));
		}

		private void Handle_BSDP_Select_Request(Guid clientid, DHCPPacket request)
		{
			var serverIP = NetbootBase.NetworkManager.ServerManager.GetEndPoint(Clients[clientid].Server, Clients[clientid].Socket);
			var encaps = request.GetEncOptions((byte)DHCPOptions.VendorSpecificInformation);
			if (!encaps[(byte)BSDPVendorEncOptions.ServerIdentifier].AsIPAddress().Equals(serverIP))
				return;

			NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
				string.Format("Got {0}[Select] from Client: {1}", request.GetMessageType(), clientid));

			// NetbootBase.NetworkManager.ServerManager.Send(Clients[clientid].Server,	Clients[clientid].Socket, Clients[clientid].Client, bytes);
		}

		public void Handle_BSDP_List_Request(Guid clientid, DHCPPacket request)
		{
			NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
				string.Format("Got {0}[List] from Client: {1}", request.GetMessageType(), clientid));
		}

		public void HeartBeat()
        {
        }
    }
}
