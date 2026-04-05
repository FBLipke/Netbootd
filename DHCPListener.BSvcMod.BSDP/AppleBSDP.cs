using Netboot.Common;
using Netboot.Module.DHCPListener;
using System.Net;
using System.Text;
using System.Xml;

namespace DHCPListener.BSvcMod.BSDP
{
    public class AppleBSDP : BootService
    {
        public AppleBSDP(XmlNode xml) : base(xml)
        {

            ServerType = BootServerType.AppleBootServer;
            DHCPListenerBase.RegisterBootService(this, ServerType, Environment.MachineName);

            ReadBootFile(xml);
        }

        public override void Handle_Bootp_Request(DHCPPacket requestPacket, Guid server, Guid socket, Guid client)
        {
            var clientId = CreateClientId(requestPacket);

            switch (requestPacket.GetVendorIdent)
            {
                case DHCPVendorID.AAPLBSDPC:
                    Clients[clientId] = new BSDPClient(false, requestPacket, server, socket, client);
                    Handle_BootService_Request(clientId, Clients[clientId].Request);
                    break;
                default:
                    return;
            }

            Clients[clientId].Response.CommitOptions();

            var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientId].Server,
                Clients[clientId].Socket, Clients[clientId].Client);

            if (!requestPacket.IsRelayed)
                endpoint.Address = requestPacket.GatewayIP;
            else if (endpoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
                endpoint.Address = IPAddress.Broadcast;

            NetbootBase.NetworkManager.ServerManager.Send(Clients[clientId].Server,
                Clients[clientId].Socket, Clients[clientId].Client, endpoint, Clients[clientId].Response.Buffer.GetBuffer());
        }

        public override void Handle_DHCP_Inform(Guid clientid, DHCPPacket request)
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
           NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
                string.Format("Got {0}[Select] from Client: {1}", request.GetMessageType(), clientid));
        }

        public void Handle_BSDP_List_Request(Guid clientid, DHCPPacket request)
        {
            NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
                string.Format("Got {0}[List] from Client: {1}", request.GetMessageType(), clientid));
        }
    }
}
