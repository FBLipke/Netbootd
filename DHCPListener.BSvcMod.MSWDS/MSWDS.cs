using Netboot.Common;
using Netboot.Common.Common.Definitions;
using Netboot.Module.DHCPListener;
using System.Buffers.Binary;
using System.Net;
using System.Text;
using System.Xml;

namespace DHCPListener.BSvcMod.MSWDS
{
    public class MSWDS : BootService
    {
        private string bcdFile { get; set; } = string.Empty;

        private ushort PollInterval { get; set; } = 10;

        private ushort RetryCount { get; set; } = 3;

        private bool ServerSelection { get; set; } = false;

        private PXEPromptOptionValues PromptAction { get; set; } = PXEPromptOptionValues.OptIn;

        public MSWDS(XmlNode xml) : base(xml)
        {
            ServerType = BootServerType.WindowsDeploymentServer;
            DHCPListenerBase.RegisterBootService(this, ServerType, Environment.MachineName);

            ReadBootFile(xml);

            var dhcpNodes = xml.SelectNodes("DHCP");
            foreach (XmlNode item in dhcpNodes)
            {
                var behavior = (BootServerType)item.ValueAsUint16("behavior");
                if (behavior == ServerType)
                {
                    PollInterval = item.ValueAsUint16("PollInterval");
                    RetryCount = item.ValueAsUint16("RetryCount");
                    PromptAction = (PXEPromptOptionValues)item.ValueAsByte("PromptAction");

                    ServerSelection = item.ValueAsByte("ServerSelection") != 0;

                    #region "DHCP Options"
                    var dhcpoptions = item.SelectNodes("Option");
                    foreach (XmlNode option in dhcpoptions)
                    {
                        var opt252 = option.ValueAsByte("id");
                        if (opt252 == (byte)252)
                            bcdFile = option.InnerText;
                    }
                    #endregion
                }
            }
        }

        public override void Handle_Bootp_Request(DHCPPacket requestPacket, Guid server, Guid socket, Guid client)
        {
            var clientId = CreateClientId(requestPacket);

            switch (requestPacket.GetVendorIdent)
            {
                case DHCPVendorID.PXEClient:
                    if (!HasBootItem(requestPacket))
                        return;

                    Clients[clientId] = new WDSClient(false, requestPacket, server, socket, client);

                    Handle_BootService_Request(clientId, Clients[clientId].Request);
                    break;
                default:
                    return;
            }

            Clients[clientId].Response.CommitOptions();

            var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientId].Server, Clients[clientId].Socket, Clients[clientId].Client);
            if (!requestPacket.IsRelayed)
                endpoint.Address = requestPacket.GatewayIP;
            else if (endpoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
                endpoint.Address = IPAddress.Broadcast;

            NetbootBase.NetworkManager.ServerManager.Send(Clients[clientId].Server, Clients[clientId].Socket, Clients[clientId].Client, endpoint,
                    Clients[clientId].Response.Buffer.GetBuffer());
        }

        public override void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
        {
            NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
                string.Format("Got WDS {0} request from Client: {1}", request.GetMessageType(), clientid));

            ((IWDSClient)Clients[clientid]).Handle_WDS_Request();


            var bcd = string.Empty;
            var filename = string.Empty;

            switch (Clients[clientid].Architecture)
            {
                case Architecture.X86PC:
                    bcd = bcdFile.Replace("#arch#", "x86");
                    filename = Bootfile.Replace("#arch#", "x86");
                    break;
                case Architecture.EFI_IA32:
                    bcd = bcdFile.Replace("#arch#", "efi");
                    filename = Bootfile.Replace("#arch#", "efi");
                    break;
                case Architecture.EFIByteCode:
                    filename = Bootfile.Replace("#arch#", "efi");
                    bcd = bcdFile.Replace("#arch#", "efi");
                    break;
                case Architecture.EFI_x8664:
                    bcd = bcdFile.Replace("#arch#", "x64");
                    filename = Bootfile.Replace("#arch#", "x64");
                    break;
                default:
                    filename = Bootfile.Replace("#arch#", "x86");
                    bcd = bcdFile.Replace("#arch#", "x86");
                    break;
            }


            ((IWDSClient)Clients[clientid]).Handle_WDS_Options();
            Clients[clientid].Response.FileName = filename;
            Clients[clientid].Response.AddOption(new((byte)252, bcd, Encoding.ASCII));
        }
    }
}
