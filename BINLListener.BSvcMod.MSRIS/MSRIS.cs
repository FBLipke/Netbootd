/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Netboot.Common;
using Netboot.Common.Common.Definitions;
using Netboot.Module.BINLListener;
using Netboot.Module.DHCPListener;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace BINLListener.BSvcMod.MSRIS
{
    public class MSRIS : IBINLService
    {
        public Dictionary<string, BINLClient> Clients { get; set; } = [];
        public BINLMessageTypes MessageType { get; set; }
        public string ServiceName { get; set; } = "MSRIS";
        private string RootPath { get; set; } = "TFTPRoot";
        private string DirName { get; set; } = "OSChooser";
        private string Language { get; set; } = "englisch";
        private string OSCFileName { get; set; } = "welcome.osc";
        private bool NTLMV2Enabled { get; set; } = false;

        public MSRIS(XmlNode xml)
        {
            MessageType = BINLMessageTypes.RequestUnsigned;
            BINLListenerBase.RegisterBINLService(this, BINLMessageTypes.Negotiate, "MSRIS");
            BINLListenerBase.RegisterBINLService(this, BINLMessageTypes.RequestUnsigned, "MSRIS");
            NetbootBase.Log("I", "BINLListener[MSRIS]", "MSRIS BINL Service registered for Negotiate, RequestUnsigned");
        }

        private static long NTQuerySystemTime()
        {
            var nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        private static byte[] NTLMChallenge()
        {
            var SysTime = BitConverter.GetBytes(NTQuerySystemTime());
            var Seed = SysTime[1] + 1 << 0 | SysTime[2] + 0 << 8 | SysTime[3] - 1 << 16 | SysTime[4] + 0 << 24;
            Seed *= 0x100;
            var rand = new Random((int)Seed);
            var ulChallenge = new uint[2];
            var ulNegate = (uint)rand.Next();
            for (var i = 0; i < ulChallenge.Length; i++)
                ulChallenge[i] = (uint)rand.Next();
            var x = ulNegate & 0x1;
            var y = ulNegate & 0x2;
            if (x == 1) ulChallenge[0] |= 0x80000000;
            if (y == 1) ulChallenge[1] |= 0x80000000;
            var challenge = new byte[2 * sizeof(uint)];
            var chal0 = BitConverter.GetBytes(ulChallenge[0]);
            var chal1 = BitConverter.GetBytes(ulChallenge[1]);
            Array.Copy(chal0, 0, challenge, 0, chal0.Length);
            Array.Copy(chal1, 0, challenge, sizeof(uint), chal1.Length);
            return challenge;
        }

        public void Setup(XmlNode xmlConfigNode)
        {
            var binlRoot = xmlConfigNode.Attributes.GetNamedItem("rootdir")?.Value;
            RootPath = !string.IsNullOrEmpty(binlRoot) ? Path.Combine(Directory.GetCurrentDirectory(), binlRoot) : Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot");
            var osclang = xmlConfigNode.Attributes.GetNamedItem("osclang")?.Value;
            if (!string.IsNullOrEmpty(osclang)) Language = osclang;
            var oscfile = xmlConfigNode.Attributes.GetNamedItem("oscfile")?.Value;
            if (!string.IsNullOrEmpty(oscfile)) OSCFileName = oscfile;
            Directory.CreateDirectory(Path.Combine(RootPath, "Setup", "Images"));
            Directory.CreateDirectory(Path.Combine(RootPath, "Setup", "OSChooser", "i386"));
            NetbootBase.Log("I", "BINLListener[MSRIS]", $"RootPath: {RootPath}, Language: {Language}, OSCFile: {OSCFileName}");
        }

        private string Handle_OSCScreen_Request(BINLPacket packet)
        {
            var OSChooserDir = new DirectoryInfo(Path.Combine(RootPath, DirName));
            var directories = OSChooserDir.GetDirectories();
            var oscml = new StringBuilder("");

            if (directories.Count() > 32)
            {
                oscml.AppendLine("<OSCML>");
                oscml.AppendLine("<META KEY=*** HREF=\"LOGIN\">");
                oscml.AppendLine("<META KEY=*** ACTION=\"REBOOT\">");
                oscml.AppendLine("<TITLE>  Client Installation Wizard                                           Welcome</TITLE>");
                oscml.AppendLine("<FOOTER> Select a language and press [ENTER] to continue </FOOTER>");
                oscml.AppendLine("<BODY left=5 right=75><BR><BR>");
                oscml.AppendLine("Select a language from the list below.  The language you select determines which");
                oscml.AppendLine("language-specific operating system choices and tools are offered for installation.<BR>");
                oscml.AppendLine("<FORM ACTION=\"WELCOME\">");
                oscml.AppendFormat("<SELECT NAME=\"LANGUAGE\" SIZE={0}>", directories.Count());
                foreach (var langDir in directories)
                    if (langDir.Name != "i386" && langDir.Name != "amd64")
                        oscml.AppendFormat($"<OPTION VALUE=\"{langDir.Name.ToUpper()}\"> {langDir.Name}");
                oscml.AppendLine("</SELECT>");
                oscml.AppendLine("</FORM>");
                oscml.AppendLine("</BODY>");
                oscml.AppendLine("</OSCML>");
            }
            else
            {
                try
                {
                    var screen = Encoding.ASCII.GetString(packet.Data);
                    var filePath = Path.Combine(OSChooserDir.FullName, string.IsNullOrEmpty(screen) ? OSCFileName : Path.Combine(Language, $"{screen.ToLowerInvariant()}.osc"));
                    var fileContent = File.ReadAllText(filePath);
                    NetbootBase.Log("I", "BINLListener[MSRIS]", $"OSChooser Screen Request: {screen.ToLowerInvariant()}");
                    fileContent = fileContent.Replace("\r\n", "\n");
                    var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    var hostname = Environment.MachineName;
                    if (!domain.Contains('.') && !string.IsNullOrEmpty(domain)) domain = string.Join(".", hostname, domain);
                    fileContent = fileContent.Replace("%MACHINEDOMAIN%", string.IsNullOrEmpty(domain) ? hostname : domain);
                    fileContent = fileContent.Replace("%SERVERDOMAIN%", string.IsNullOrEmpty(domain) ? hostname : domain);
                    fileContent = fileContent.Replace("%NTLMV2Enabled%", NTLMV2Enabled ? "1" : "0");
                    fileContent = fileContent.Replace("%SERVERNAME%", hostname);
                    fileContent = fileContent.Replace("%ServerUTCFileTime%", string.Format("{0}", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    oscml.Append(fileContent);
                }
                catch (FileNotFoundException ex)
                {
                    oscml.Append($"<OSCML> \\r\\n\t\t\t<META KEY=\"***\" ACTION=\"REBOOT\"><META KEY=\"***\" HREF=\"LOGIN\">" +
                        "<TITLE>  Client Installation Wizard                                           Error</TITLE>" +
                        $"<FOOTER>[F3] restart computer [ENTER] Continue</FOOTER>" +
                        $"<BODY left=5 right=75><BR>The requested file \"{ex.FileName}\" was not found on the Server.</BODY></OSCML>");
                }
            }
            return oscml.ToString();
        }

        public void Handle_RQU_Request(Guid server, Guid socket, string client, BINLPacket packet)
        {
            var oscfile = Handle_OSCScreen_Request(packet);
            var response = new BINLPacket(BINLMessageTypes.ResponseUnsigned)
            {
                Sequence = packet.Sequence,
                Fragment = packet.Fragment,
                TotalFragments = packet.TotalFragments,
                SignLength = packet.SignLength,
                Sign = packet.Sign,
                Data = Encoding.ASCII.GetBytes(oscfile)
            };
            response.Length = (uint)response.Buffer.Length - 8;
            if (Clients.TryGetValue(client, out var binlClient)) binlClient.Response = response;
        }

        public void Handle_NEG_Request(Guid server, Guid socket, string client, BINLPacket packet)
        {
            var ntlmrequest = packet.NTLMSSP;
            var response = new NTLMSSPPacket(ServiceName, ntlmssp_message_type.Challenge);
            switch (ntlmrequest.MessageType)
            {
                case ntlmssp_message_type.Negotiate:
                    NetbootBase.Log("I", "BINLListener[MSRIS]", string.Format("[I] NTLM Negotiate! (Flags: {0})", ntlmrequest.Flags));
                    response.Flags = ntlmrequest.Flags;
                    response.Challenge = Netboot.Common.Functions.NTLMChallenge();
                    response.Context = new byte[8];
                    break;
                case ntlmssp_message_type.Challenge:
                    NetbootBase.Log("I", "BINLListener[MSRIS]", "[I] NTLM Challenge!");
                    break;
                case ntlmssp_message_type.Authenticate:
                    NetbootBase.Log("I", "BINLListener[MSRIS]", "[I] NTLM Authenticate!");
                    break;
                default:
                    NetbootBase.Log("W", "BINLListener[MSRIS]", "[I] invalid Type!");
                    return;
            }
        }

        void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
        {
            if (!Clients.TryGetValue(clientId, out var value))
                Clients.Add(clientId, new BINLClient(false, serverId, socketId, Guid.Parse(clientId), null));
            else
                value.Client = Guid.Parse(clientId);
        }

        public void Handle_BINL_Request(Guid client, BINLPacket requestPacket)
        {
            var clientId = client.ToString();
            Handle_BINL_Request(clientId, requestPacket);
        }

        public void Handle_BINL_Request(string client, BINLPacket requestPacket)
        {
            AddClient(client, ServiceName, null, Guid.Empty, Guid.Empty);
            switch (requestPacket.MessageType)
            {
                case BINLMessageTypes.RequestUnsigned:
                    NetbootBase.Log("I", "BINLListener[MSRIS]", $"==== (OSC-Header (Seq: {requestPacket.Sequence} Frag: {requestPacket.Fragment}/{requestPacket.TotalFragments}) ====");
                    Handle_RQU_Request(Guid.Empty, Guid.Empty, client, requestPacket);
                    break;
                case BINLMessageTypes.Negotiate:
                    Handle_NEG_Request(Guid.Empty, Guid.Empty, client, requestPacket);
                    break;
                default:
                    break;
            }
        }

        public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
        {
            var requestPacket = new BINLPacket(memoryStream.GetBuffer());
            var clientId = client.ToString();
            AddClient(clientId, ServiceName, null, server, socket);
            switch (requestPacket.MessageType)
            {
                case BINLMessageTypes.RequestUnsigned:
                    NetbootBase.Log("I", "BINLListener[MSRIS]", $"==== (OSC-Header (Seq: {requestPacket.Sequence} Frag: {requestPacket.Fragment}/{requestPacket.TotalFragments}) ====");
                    Handle_RQU_Request(server, socket, clientId, requestPacket);
                    break;
                case BINLMessageTypes.Negotiate:
                    Handle_NEG_Request(server, socket, clientId, requestPacket);
                    break;
                default:
                    break;
            }
        }

        public void HeartBeat() { }
    }
}