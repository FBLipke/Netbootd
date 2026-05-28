using Netboot.Common;
using Netboot.Common.Utility;
using Netboot.Module.DHCPListener;
using System.Net;
using System.Reflection;
using System.Xml;

namespace DHCPListener.BSvcMod.MSRIS
{
    public class MSRIS : BootService
    {
        private string Bootfile { get; set; } = string.Empty;

        public void LoadUtilitiyModule()
        {
            #region "Load Service Modules"

            var _types = this.GetType().Assembly.GetTypes();
            var retvalColl = from t in _types
                             where (t.GetInterfaces()
                                .Contains(typeof(IUtility))) && t.IsAbstract == false
                             let moduleName = this.GetType().Assembly.GetName().Name
                             select (t, moduleName);

            foreach (var (t, name) in retvalColl)
            {
                try
                {
                    var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance,
                        null, null, null) as IUtility;

                    if (b == null)
                        continue;

                    NetbootBase.UtilProviders.Add(b.Name, b);
                }
                catch (MissingMethodException ex)
                {
                    NetbootBase.Log("I", "Netbootd", "Installation completed...");
                }
            }

            Console.WriteLine("");
        }
        #endregion

        public MSRIS(XmlNode xml) : base(xml)
        {
            ServerType = BootServerType.MicrosoftWindowsNT;
            DHCPListenerBase.RegisterBootService(this, ServerType, Environment.MachineName);

            ReadBootFile(xml);

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

            DHCPListenerBase.RegisterBootService(this, ServerType, Environment.MachineName);

            LoadUtilitiyModule();
        }

        public override void Handle_Bootp_Request(DHCPPacket requestPacket, Guid server, Guid socket, Guid client)
        {
            var clientId = CreateClientId(requestPacket);

            switch (requestPacket.GetVendorIdent)
            {
                case DHCPVendorID.PXEClient:
                    if (!HasBootItem(requestPacket))
                        return;

                    Clients[clientId] = new RISClient(false, requestPacket, server, socket, client);

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

        public override void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
        {
            NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
                string.Format("Got RIS {0} request from Client: {1}", request.GetMessageType(), clientid));

            SelectBootfile(clientid);
        }
    }
}
