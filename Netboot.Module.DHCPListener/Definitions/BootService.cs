using Netboot.Common;
using Netboot.Common.Common.Definitions;
using Netboot.Module.DHCPListener.Interfaces;
using System.Xml;

namespace Netboot.Module.DHCPListener
{
    public class BootService : IBootService, IDHCPListener
    {
        protected Dictionary<Guid, IDHCPClient> Clients = [];

        protected Dictionary<string, BootServer> bootServers = [];

        public BootServerType ServerType { get; set; }

        public string Bootfile { get; set; } = string.Empty;

        public BootService(XmlNode xml)
        {
            DHCPListenerBase.BootServiceRequest += (sender, e) =>
            {
                Handle_Listener_Request(e.Server, e.Socket, e.Client, e.Request);
            };

            var hostname = Environment.MachineName;
            bootServers.Add(Guid.Empty.ToString(), new BootServer(hostname, ServerType));
        }

        public virtual void Handle_BootService_Request(string client, DHCPPacket requestPacket)
            => Handle_BootService_Request(Guid.Parse(client), requestPacket);

        public virtual void Handle_BootService_Request(Guid client, DHCPPacket requestPacket)
        {
            switch (requestPacket.GetMessageType())
            {
                case DHCPMessageType.Discover:
                    Handle_DHCP_Discover(client, requestPacket);
                    break;
                case DHCPMessageType.Request:
                    Handle_DHCP_Request(client, requestPacket);
                    break;
                case DHCPMessageType.Inform:
                    Handle_DHCP_Inform(client, requestPacket);
                    break;
                default:
                    return;
            }
        }

        public void ReadBootFile(XmlNode xml)
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
        }

        public void SelectBootfile(Guid clientId)
        {
            var filename = string.Empty;

            switch (Clients[clientId].Architecture)
            {
                case Architecture.X86PC:
                    filename = Bootfile.Replace("#arch#", "x86");
                    break;
                case Architecture.EFI_IA32:
                    filename = Bootfile.Replace("#arch#", "efi");
                    break;
                case Architecture.EFIByteCode:
                    filename = Bootfile.Replace("#arch#", "efi");
                    break;
                case Architecture.EFI_x8664:
                    filename = Bootfile.Replace("#arch#", "x64");
                    break;
                default:
                    filename = Bootfile.Replace("#arch#", "x86");
                    break;
            }
            
            Clients[clientId].Response.FileName = filename;
        }

        public virtual void Handle_DHCP_Discover(Guid clientid, DHCPPacket request)
        {
        }

        public virtual void Handle_DHCP_Request(Guid clientid, DHCPPacket request)
        {
        }

        public virtual void Handle_DHCP_Inform(Guid clientid, DHCPPacket request)
        {
        }

        public virtual void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
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
                    Handle_Bootp_Request(requestPacket, server, socket, client);

                    break;
                case BOOTPOPCode.BootReply:
                    Handle_Bootp_Reply(requestPacket, server, socket, client);

                    break;
                default:
                    return;
            }
        }

        public virtual void HeartBeat()
        {
            bootServers.Clear();
            bootServers.Add(Guid.Empty.ToString(), new(Environment.MachineName, ServerType));

            foreach (var key in DHCPListenerBase.Bootservices.Keys.ToList())
            {
                if (key == BootServerType.PXEBootstrapServer)
                    continue;

                bootServers.Add(Guid.NewGuid().ToString(), new(Environment.MachineName, key));
            }
        }

        public virtual void Handle_Bootp_Request(DHCPPacket requestPacket, Guid server, Guid socket, Guid client)
        {
        }

        public virtual void Handle_Bootp_Reply(DHCPPacket requestPacket, Guid server, Guid socket, Guid client)
        {
            var srvIP = Clients[client].Request.Options[(byte)DHCPOptions.ServerIdentifier].AsIPAddress();

            NetbootBase.Log("I", string.Format("DHCPListener[{0}]", ServerType),
                string.Format("Received {3} {0} reply from DHCP Server: {1} ({2})", requestPacket.GetMessageType(), client, srvIP, requestPacket.IsRelayed ? "relayed" : string.Empty));
        }

        public Guid CreateClientId(DHCPPacket packet)
        {
            var clientId = packet.HardwareAddress.ToGuid();
            var opt = packet.GetOption((byte)DHCPOptions.UuidGuidBasedClientIdentifier).Data;
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

            return clientId;
        }

        public bool HasBootItem(DHCPPacket packet)
        {
            if (!packet.HasOption(43))
                return false;

            var enCapOpts = packet.GetEncOptions(43);
            if (enCapOpts.ContainsKey(71))
            {
                var bsType = (BootServerType)enCapOpts[71].AsUInt16();
                if (bsType != ServerType)
                    return false;
            }

            return true;
        }
    }
}