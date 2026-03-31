using Netboot.Common;
using Netboot.Module.DHCPListener;
using System.Net;
using System.Text;
using System.Xml;
using static DHCPListener.BSvcMod.RBCP.Definitions.Definitions;

namespace DHCPListener.BSvcMod.RBCP
{
    public class PxeRBCP : BootService
    {
        public byte DiscoveryControl { get; private set; } = 3;

        public byte MenueTimeout { get; private set; } = 10;

        public string[] MenuePrompt { get; private set; } =
            { "Select Server...", "Press [F8] to boot from Network or [esc] to cancel..." };

        public byte MulticastDelay { get; private set; } = 4;

        public byte MulticastTimeout { get; private set; } = 10;

        public ushort MulticastCPort { get; private set; } = 4001;

        public ushort MulticastSPort { get; private set; } = 69;

        public IPAddress MulticastDiscoveryAddress { get; private set; } = IPAddress.Any;

        public PxeRBCP(XmlNode xml) : base(xml)
        {
            ServerType = BootServerType.PXEBootstrapServer;
            DiscoveryControl = byte.Parse(xml.Attributes.GetNamedItem("discovery").Value);
            MulticastDelay = byte.Parse(xml.Attributes.GetNamedItem("mcstartdelay").Value);
            MulticastTimeout = byte.Parse(xml.Attributes.GetNamedItem("mctimeout").Value);
            MulticastDiscoveryAddress = IPAddress.Parse(xml.Attributes.GetNamedItem("mcaddr").Value);
            MenueTimeout = byte.Parse(xml.Attributes.GetNamedItem("menuetimeout").Value);
            MenuePrompt = xml.Attributes.GetNamedItem("prompt").Value.Split(';');

            MulticastSPort = ushort.Parse(xml.Attributes.GetNamedItem("mcsport").Value);
            MulticastCPort = ushort.Parse(xml.Attributes.GetNamedItem("mccport").Value);
        }

        public override void Handle_Bootp_Request(DHCPPacket requestPacket, Guid server, Guid socket, Guid client)
        {
            var clientId = CreateClientId(requestPacket);


            switch (requestPacket.GetVendorIdent)
            {
                case DHCPVendorID.PXEClient:
                    if (requestPacket.GetMessageType() != DHCPMessageType.Discover)
                        return;

                    Clients[clientId] = new RBCPClient(false, requestPacket, server, socket, client);

                    Handle_BootService_Request(clientId, Clients[clientId].Request);
                    break;
                default:
                    return;
            }

            Clients[clientId].Response.CommitOptions();

            var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(Clients[clientId].Server,
                Clients[clientId].Socket, Clients[clientId].Client);

            if (endpoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
                endpoint.Address = IPAddress.Broadcast;

            NetbootBase.NetworkManager.ServerManager.Send(Clients[clientId].Server,
                Clients[clientId].Socket, Clients[clientId].Client, endpoint, Clients[clientId].Response.Buffer.GetBuffer());
        }

        public override void Handle_DHCP_Discover(Guid clientid, DHCPPacket request)
        {
            Clients[clientid].Response.AddOption(new((byte)DHCPOptions.VendorSpecificInformation,
            [
                new ((byte) PXEOptions.MulticastTFTPDelay, MulticastDelay),
                new ((byte) PXEOptions.MulticastTFTPTimeout, MulticastTimeout),
                new ((byte) PXEOptions.DiscoveryMulticastAddress, MulticastDiscoveryAddress),
                GenerateBootServersList(bootServers),
                GenerateBootMenuePrompt(MenueTimeout, MenuePrompt),
                GenerateBootMenue(bootServers),
                new ((byte) PXEOptions.MulticastServerPort, MulticastSPort),
                new ((byte) PXEOptions.MulticastClientPort, MulticastCPort),
                new ((byte) PXEOptions.DiscoveryControl, DiscoveryControl),
                new ((byte) PXEOptions.End)
            ]));
        }

        public static DHCPOption<byte> GenerateBootServersList(Dictionary<string, BootServer> serverlist)
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

            return new((byte)PXEOptions.BootServer, serverlistBlock);
        }

        public static DHCPOption<byte> GenerateBootMenue(Dictionary<string, BootServer> servers)
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
            
            return new((byte)PXEOptions.BootMenue, menubuffer);
        }

        public static DHCPOption<byte> GenerateBootMenuePrompt(byte timeout, string[] menueprompt)
        {
            var prompt = Encoding.ASCII.GetBytes(timeout == byte.MaxValue ?  menueprompt.First() :
                menueprompt.Last());

            var promptbuffer = new byte[1 + prompt.Length];
            var offset = 0;

            #region "Timeout"
            promptbuffer[offset] = timeout;
            offset += sizeof(byte);
            #endregion

            #region "Prompt"
            Array.Copy(prompt, 0, promptbuffer, offset, prompt.Length);
            #endregion

            return new DHCPOption<byte>((byte)PXEOptions.MenuPrompt, promptbuffer);
        }
    }
}
