using Netboot.Common;
using Netboot.Module.DHCPListener;
using Netboot.Module.DHCPListener.Interfaces;
using System.Net;
using System.Text;
using System.Xml;

namespace DHCPListener.BSvcMod.BSDP
{
	public class AppleBSDP: IBootService, IDHCPListener
    {
		Dictionary<Guid, IBSDPClient> Clients = [];

		public BootServerType ServerType { get; set; } = BootServerType.Apple;

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
							#region Get the UUID (GUID) of the Client and add him
							var clientId = Guid.Empty;


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
                            NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
								string.Format("Got {0} ({1}) request from Client: {2}", requestPacket.GetMessageType(),
								requestPacket.GetVendorIdent, client));
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
			NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
				string.Format("Got {0} request from Client: {1}", requestPacket.GetMessageType(),
					client));

			switch (requestPacket.GetMessageType())
			{
				case DHCPMessageType.Discover:
					Handle_DHCP_Discover(client, requestPacket);
					break;
				case DHCPMessageType.Inform:
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
			Clients[clientid].Response.AddOption(new DHCPOption<byte>((byte)DHCPOptions.VendorClassIdentifier, "AAPLBSDPC", Encoding.ASCII));

			var bytes = Clients[clientid].Response.Buffer.GetBuffer();
			Clients[clientid].Response.CommitOptions();

			var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientid].Server, Clients[clientid].Socket, Clients[clientid].Client);
			if (endpoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
				endpoint.Address = IPAddress.Broadcast;

			NetbootBase.NetworkManager.ServerManager.Send(Clients[clientid].Server,
				Clients[clientid].Socket, Clients[clientid].Client, endpoint, bytes);
		}

		public void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
		{
		}

        public void HeartBeat()
        {
        }
    }
}
