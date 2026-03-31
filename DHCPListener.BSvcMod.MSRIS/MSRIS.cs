using Netboot.Common;
using Netboot.Module.DHCPListener;
using Netboot.Module.DHCPListener.Interfaces;
using System.Text;
using System.Xml;

namespace DHCPListener.BSvcMod.MSRIS
{
    public class MSRIS : IBootService, IDHCPListener
    {
        Dictionary<Guid, IRISClient> Clients = [];

        public BootServerType ServerType { get; set; } = BootServerType.MicrosoftWindowsNT;

        private string Bootfile { get; set; } = string.Empty;

        public MSRIS(XmlNode xml)
        {
            var bootfiles = xml.SelectSingleNode("Bootfiles").ChildNodes;
            foreach (XmlNode item in bootfiles)
            {
                var behavior = (BootServerType)item.ValueAsUint16("behavior");
                if (behavior == ServerType)
                {
                    Bootfile = item.InnerText;
                    break;
                }
            }

            var dhcpNodes = xml.SelectNodes("DHCP");
            foreach (XmlNode item in dhcpNodes)
            {
                var behavior = (BootServerType)item.ValueAsUint16("behavior");
                if (behavior == ServerType)
                {
                    #region "DHCP Options"
                    #endregion
                }
            }

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

                            Clients[clientId] = new RISClient(clientId, requestPacket, server, socket, client);
                            #endregion

                            var serverIP = NetbootBase.NetworkManager.ServerManager.GetEndPoint(server, socket).Address;
                            Clients[clientId].Response = Clients[clientId].Request.CreateResponse(serverIP);

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

        public void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
        {
            NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
                string.Format("Got RIS {0} request from RIS Client: {1}", request.GetMessageType(), clientid));

            var filename = string.Empty;

            switch (Clients[clientid].Architecture)
            {
                case Netboot.Module.DHCPListener.Architecture.X86PC:
                    filename = Bootfile.Replace("#arch#", "x86");
                    break;
                case Netboot.Module.DHCPListener.Architecture.EFI_IA32:
                    filename = Bootfile.Replace("#arch#", "efi");
                    break;
                case Netboot.Module.DHCPListener.Architecture.EFIByteCode:
                    filename = Bootfile.Replace("#arch#", "efi");
                    break;
                case Netboot.Module.DHCPListener.Architecture.EFI_x8664:
                    filename = Bootfile.Replace("#arch#", "x64");
                    break;
                default:
                    filename = Bootfile.Replace("#arch#", "x86");
                    break;
            }

            Clients[clientid].Response.FileName = filename;
            Clients[clientid].Response.AddOption(new((byte)DHCPOptions.VendorClassIdentifier, "PXEClient", Encoding.ASCII));

            var bytes = Clients[clientid].Response.Buffer.GetBuffer();

            Clients[clientid].Response.CommitOptions();

            var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientid].Server, Clients[clientid].Socket, Clients[clientid].Client);
            NetbootBase.NetworkManager.ServerManager.Send(Clients[clientid].Server, Clients[clientid].Socket, Clients[clientid].Client, endpoint, bytes);
        }

        public void Handle_DHCP_Discover(Guid clientid, DHCPPacket request)
        {
        }
    }
}
