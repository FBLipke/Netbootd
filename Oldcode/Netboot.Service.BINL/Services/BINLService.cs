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

using Netboot.Network.Packet;
using Netboot.Service.BINL.Network.Client;
using Netboot.Service.BINL.Network.Definitions;
using Netboot.Service.BINL.Network.Packet;
using NetBoot.Common.Common;
using NetBoot.Common.Common.Definitions;
using NetBoot.Common.Network.EventHandler;
using NetBoot.Common.Network.Interfaces;
using NetBoot.Common.Network.Sockets.Definition;
using NetBoot.Common.System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Netboot.Service.BINL.Services
{
    public class BINLService : IService
	{
		public BINLService(string serviceType)
		{
			RootPath = string.Empty;
			ServiceType = serviceType;
		}

		public List<ushort> Ports { get; set; } = [];

		public SocketProtocol Protocol { get; set; } = SocketProtocol.UDP;

		public string RootPath { get; private set; } = "TFTPRoot";

		public string DirName { get; private set; } = "OSChooser";

		public string OSCFileName { get; private set; } = "welcome.osc";

		public string Language { get; private set; } = "englisch";

		public bool NTLMV2Enabled { get; private set; } = false;

		public string ServiceType { get; }

		public Dictionary<string, BINLClient> Clients { get; set; } = [];

		public event IService.AddServerEventHandler? AddServer;
		public event IService.ServerSendPacketEventHandler? ServerSendPacket;
        public event IService.PrintMessageEventHandler? PrintMessage;

        public void Dispose()
		{
			foreach (var client in Clients.Values)
				client.Dispose();

			Ports.Clear();
		}

		public void Setup(XmlNode xmlConfigNode)
		{
			var binlRoot = xmlConfigNode.Attributes.GetNamedItem("rootdir").Value;
			if (!string.IsNullOrEmpty(binlRoot))
				RootPath = Path.Combine(Directory.GetCurrentDirectory(), binlRoot);
			else
				RootPath = Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot");

			var osclang = xmlConfigNode.Attributes.GetNamedItem("osclang").Value;
			if (!string.IsNullOrEmpty(osclang))
				Language = osclang;

			var oscfile = xmlConfigNode.Attributes.GetNamedItem("oscfile").Value;
			if (!string.IsNullOrEmpty(oscfile))
				OSCFileName = oscfile;

			Directory.CreateDirectory(Path.Combine(RootPath, "Setup", "Images"));
			Directory.CreateDirectory(Path.Combine(RootPath, "Setup", "OSChooser", "i386"));

		}

		string Handle_OSCScreen_Request(ref BINLPacket packet)
		{
			var OSChooserDir = new DirectoryInfo(Path.Combine(RootPath, DirName));
			var directories = OSChooserDir.GetDirectories();

			#region "Get the OSCML screen"
			var oscml = new StringBuilder("");

			// We may have more than one OSes with different languages on the server.


			// WELCOME\nLANGUAGE=ENGLISH\n

			if (directories.Count() > 32)
			{
				oscml.AppendLine("<OSCML>");
				oscml.AppendLine("<META KEY=ENTER HREF=\"LOGIN\">");
				oscml.AppendLine("<META KEY=F3 ACTION=\"REBOOT\">");
				oscml.AppendLine("<TITLE>  Client Installation Wizard                                           Welcome</TITLE>");
				oscml.AppendLine("<FOOTER> Select a language and press [ENTER] to continue </FOOTER>");
				oscml.AppendLine("<BODY left=5 right=75><BR><BR>");

				oscml.AppendLine("Select a language from the list below.  The language you select determines which");
				oscml.AppendLine("language-specific operating system choices and tools are offered for installation.<BR>");

				oscml.AppendLine("<FORM ACTION=\"WELCOME\">");
				oscml.AppendFormat("<SELECT NAME=\"LANGUAGE\" SIZE={0}>", directories.Count());

				foreach (var Language in directories)
					if (Language.Name != "i386" && Language.Name != "amd64")
						oscml.AppendFormat($"<OPTION VALUE=\"{Language.Name.ToUpper()}\"> {Language.Name}");

				oscml.AppendLine("</SELECT>");
				oscml.AppendLine("</FORM>");
				oscml.AppendLine("</BODY>");
				oscml.AppendLine("</OSCML>");
			}
			else
			{
				string filePath = string.Empty;
				try
				{
					var screen = Encoding.ASCII.GetString(packet.Data);
					filePath = Path.Combine(OSChooserDir.FullName,
						string.IsNullOrEmpty(screen) ? OSCFileName : Path.Combine(Language,
						string.Format($"{screen.ToLowerInvariant()}.osc")));

					var fileContent = File.ReadAllText(filePath);

					PrintMessage?.Invoke(this, new($"[I] OSChooser Screen Request: {screen.ToLowerInvariant()}"));

					fileContent = fileContent.Replace("\r\n", "\n");

					var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
					var hostname = Environment.MachineName;

					if (!domain.Contains('.') && !string.IsNullOrEmpty(domain))
						domain = string.Join(".", hostname, domain); // HOSTNAME.LOCALDOMAIN

					fileContent = fileContent.Replace("%MACHINEDOMAIN%", string.IsNullOrEmpty(domain) ? hostname : domain);
					fileContent = fileContent.Replace("%SERVERDOMAIN%", string.IsNullOrEmpty(domain) ? hostname : domain);
					fileContent = fileContent.Replace("%NTLMV2Enabled%", NTLMV2Enabled ? "1" : "0");
					fileContent = fileContent.Replace("%SERVERNAME%", hostname);
					fileContent = fileContent.Replace("%ServerUTCFileTime%", string.Format("{0}", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

					oscml.Append(fileContent);
				}
				catch (FileNotFoundException ex)
				{
					var errcontent = string.Format("<OSCML> \\\r\n\t\t\t<META KEY=\\\"F3\\\" ACTION=\\\"REBOOT\\\"><META KEY=\\\"ENTER\\\" HREF=\\\"LOGIN\\\">" +
						"<TITLE>  Client Installation Wizard                                           Error</TITLE><FOOTER>[F3] restart computer [ENTER] Continue</FOOTER>" +
						"<BODY left=5 right=75><BR>The requested file \"{0}\" was not found on the Server.</BODY></OSCML>", ex.FileName);
					oscml.Append(errcontent);
				}
			}
			#endregion

			return oscml.ToString();
		}

		public void Handle_RQU_Request(Guid server, Guid socket, string client, BINLPacket packet)
		{
			var oscfile = Handle_OSCScreen_Request(ref packet);

			var response = new BINLPacket(ServiceType, BINLMessageTypes.ResponseUnsigned)
			{
				Sequence = packet.Sequence,
				Fragment = packet.Fragment,
				TotalFragments = packet.TotalFragments,
				SignLength = packet.SignLength,
				Sign = packet.Sign,
				Data = Encoding.ASCII.GetBytes(oscfile)
			};
			response.Length = (uint)response.Buffer.Length - 8;

			ServerSendPacket?.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));
		}

		public void Handle_NEG_Request(Guid server, Guid socket, string client, BINLPacket packet)
		{
			var ntlmrequest = packet.NTLMSSP;
			var response = new NTLMSSPPacket(ServiceType, ntlmssp_message_type.Challenge);

			switch (ntlmrequest.MessageType)
			{
				case ntlmssp_message_type.Negotiate:
					PrintMessage?.Invoke(this, new(string.Format("[I] NTLM Negotiate! (Flags: {0})", ntlmrequest.Flags)));
					response.Flags = ntlmrequest.Flags;
					response.Challenge = Functions.NTLMChallenge();
					response.Context = new byte[8];
					break;
				case ntlmssp_message_type.Challenge:
					PrintMessage?.Invoke(this, new(string.Format("[I] NTLM Challenge!")));
					break;
				case ntlmssp_message_type.Authenticate:
                    PrintMessage?.Invoke(this, new(string.Format("[I] NTLM Authenticate!")));
					break;
				default:
                    PrintMessage?.Invoke(this, new(string.Format("[I] invalid Type!")));
					return;
			}
		}

		void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
		{
			if (!Clients.TryGetValue(clientId, out var value))
				Clients.Add(clientId, new(clientId, serviceType, remoteEndpoint, serverId, socketId));
			else
                value.RemoteEndpoint = remoteEndpoint;
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			var request = new BINLPacket(ServiceType, e.Packet);
			var clientid = e.RemoteEndpoint.Address.ToString();

			AddClient(clientid, ServiceType, e.RemoteEndpoint, e.ServerId, e.SocketId);

			switch (request.MessageType)
			{
				case BINLMessageTypes.RequestUnsigned:
					Console.WriteLine(" ==== (OSC-Header (Seq: {0} Frag: {1}/{2}) ====", request.Sequence, request.Fragment, request.TotalFragments);
					Handle_RQU_Request(e.ServerId, e.SocketId, clientid, request);
					break;
				case BINLMessageTypes.Negotiate:
					Handle_NEG_Request(e.ServerId, e.SocketId, clientid, request);
					break;
				default:
					break;
			}
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
		{
		}

		public void Heartbeat(DateTime now)
		{
		}

		public bool Initialize(XmlNode xmlConfigNode)
		{
			if (xmlConfigNode == null)
				return false;

			return true;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}
	}
}
