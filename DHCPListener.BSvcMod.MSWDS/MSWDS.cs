using Netboot.Common;
using Netboot.Module.DHCPListener;
using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace DHCPListener.BSvcMod.MSWDS
{
	public class MSWDS : IBootService
	{
		internal Dictionary<Guid, IWDSClient> Clients = [];

		public BootServerType ServerType { get; set; } = BootServerType.WindowsDeploymentServer;

		public MSWDS()
		{
			DHCPListenerBase.BootServiceRequest += (sender, e) =>
			{
				Handle_Listener_Request(e.Server, e.Socket, e.Client, e.Request);
			};

			DHCPListenerBase.RegisterBootService(this, ServerType, Environment.MachineName);
		}

		public void Handle_BootService_Request(string client, DHCPPacket requestPacket)
			=> Handle_BootService_Request(Guid.Parse(client), requestPacket);
	
		public void Handle_BootService_Request(Guid client, DHCPPacket requestPacket)
		{
			switch (requestPacket.GetMessageType())
			{
				case DHCPMessageType.Request:
				case DHCPMessageType.Discover:
					Handle_DHCP_Request(client, requestPacket);
					break;
			}
		}

		public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
		{
			var requestPacket = new DHCPPacket(memoryStream);

			switch (requestPacket.BootpOPCode)
			{
				case BOOTPOPCode.BootRequest:
					switch (requestPacket.GetVendorIdent)
					{
						case DHCPVendorID.PXEClient:
							#region Get the UUID (GUID) of the Client and add him
							var clientId = requestPacket.HardwareAddress.ToGuid();

							if (!requestPacket.HasOption(43))
								return;

							var enCapOpts = requestPacket.GetEncOptions(43);
							if (enCapOpts.ContainsKey(71))
							{
								var bsType = (BootServerType)enCapOpts[71].AsUInt16();
								if (bsType != ServerType)
									return;
							}
							
							var opt = requestPacket.GetOption((byte)DHCPOptions.UuidGuidBasedClientIdentifier).Data;
							switch ((ClientIdentType)opt.First())
							{
								case ClientIdentType.UUID:
									var idBytes = new byte[16];
									Array.Copy(opt, 1, idBytes, 0, idBytes.Length);

									clientId = Netboot.Module.DHCPListener.Functions.AsLittleEndianGuid(idBytes);
									break;
								default:
									break;
							}

							Clients[clientId] = new WDSClient(clientId, requestPacket, server, socket, client);
							#endregion

							var serverIP = NetbootBase.NetworkManager.ServerManager.GetEndPoint(server, socket);
							Clients[clientId].Response = Clients[clientId].Request.CreateResponse(serverIP.Address);

							Handle_BootService_Request(clientId, Clients[clientId].Request);
							break;
						default:
							return;
					}

					break;
				default:
					return;
			}
		}

		public void HeartBeat()
		{

		}

		void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
		{
			NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
				string.Format("Got WDS {0} request from WDS Client: {1}", request.GetMessageType(), clientid));

			Handle_WDS_Request(clientid);
			
			Clients[clientid].Response.AddOption(new DHCPOption<byte>((byte)DHCPOptions.VendorClassIdentifier, "PXEClient", Encoding.ASCII));
			Clients[clientid].Response.AddOption(Handle_WDS_Options(clientid));

			var bytes = Clients[clientid].Response.Buffer.GetBuffer();
			Clients[clientid].Response.CommitOptions();

			var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientid].Server, Clients[clientid].Socket, Clients[clientid].Client);
			NetbootBase.NetworkManager.ServerManager.Servers[Clients[clientid].Server].Send(Clients[clientid].Socket, Clients[clientid].Client,
				endpoint, bytes);
		}

		void Handle_WDS_Request(Guid client)
		{
			var wdsData = Clients[client].Request.GetEncOptions(250);
			foreach (var wdsOption in wdsData.Values)
			{
				switch ((WDSNBPOptions)wdsOption.Option)
				{
					case WDSNBPOptions.Unknown:
						break;
					case WDSNBPOptions.Architecture:
						Clients[client].Architecture = (Architecture)wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.NextAction:
						Clients[client].NextAction = (NextActionOptionValues)wdsOption.AsByte();
						break;
					case WDSNBPOptions.PollInterval:
						Clients[client].PollInterval = wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.PollRetryCount:
						Clients[client].RetryCount = wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.RequestID:
						Clients[client].RequestId = wdsOption.AsUInt32();
						break;
					case WDSNBPOptions.VersionQuery:
						Clients[client].VersionQuery = true;
						break;
					case WDSNBPOptions.ServerVersion:
						Clients[client].ServerVersion = (NBPVersionValues)wdsOption.AsUInt32();
						break;
					case WDSNBPOptions.ReferralServer:
						Clients[client].ReferralServer = wdsOption.AsIPAddress();
						break;
					case WDSNBPOptions.PXEClientPrompt:
						Clients[client].PXEPromptAction = (PXEPromptOptionValues)wdsOption.AsByte();
						break;
					case WDSNBPOptions.PxePromptDone:
						Clients[client].PXEPromptDone = (PXEPromptOptionValues)wdsOption.AsByte();
						break;
					case WDSNBPOptions.NBPVersion:
						Clients[client].NBPVersion = (NBPVersionValues)wdsOption.AsUInt16();
						break;
					case WDSNBPOptions.ServerFeatures:
						Clients[client].ServerFeatures = wdsOption.AsUInt32();
						break;
					case WDSNBPOptions.ActionDone:
						Clients[client].ActionDone = true;

						//Clients[client].WDS.ActionDone = ServerMode == DHCPServerMode.AllowAll;
						break;
					default:
						break;
				}
			}
		}

		public DHCPOption<byte> Handle_WDS_Options(Guid client)
		{
			var options = new List<DHCPOption<byte>>
			{
				new((byte)WDSNBPOptions.NextAction, (byte)Clients[client].NextAction),
				new ((byte)WDSNBPOptions.PXEClientPrompt, (byte)Clients[client].PXEPromptDone),
				new ((byte)WDSNBPOptions.ActionDone, Convert.ToByte(Clients[client].ActionDone)),
				new ((byte)WDSNBPOptions.PollRetryCount, Clients[client].RetryCount),
			};

			var requestIDBytes = new byte[sizeof(uint)];
			BinaryPrimitives.WriteUInt32BigEndian(requestIDBytes, (uint)Clients.Count);
			options.Add(new((byte)WDSNBPOptions.RequestID, requestIDBytes));

			var polldelayBytes = new byte[sizeof(short)];
			BinaryPrimitives.WriteUInt16BigEndian(polldelayBytes, Clients[client].PollInterval);
			options.Add(new((byte)WDSNBPOptions.PollInterval, polldelayBytes));

			options.Add(new((byte)WDSNBPOptions.PXEClientPrompt, (byte)Clients[client].PXEPromptAction));
			options.Add(new((byte)WDSNBPOptions.AllowServerSelection, Convert.ToByte(Clients[client].ServerSelection)));

			switch (Clients[client].NextAction)
			{
				case NextActionOptionValues.Approval:
					options.Add(new((byte)WDSNBPOptions.Message, Clients[client].Message, Encoding.ASCII));
					break;
				case NextActionOptionValues.Referral:
					options.Add(new((byte)WDSNBPOptions.ReferralServer, Clients[client].ReferralServer));
					break;
				default:
					break;
			}

			return new(250, options);
		}

	}
}
