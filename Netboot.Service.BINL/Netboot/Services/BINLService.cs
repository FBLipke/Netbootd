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
using Netboot.Common.Definitions;
using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Packet;
using Netboot.Network.Sockets;
using Netboot.Services.Interfaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Netboot.Service.BINL
{
    public class BINLService : IService
	{
		public BINLService(string serviceType)
		{
			ServiceType = serviceType;
		}

		public List<ushort> Ports { get; set; } = [];

		public SocketProtocol Protocol { get; set; } = SocketProtocol.UDP;

		public string RootPath { get; set; }

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

		public void Handle_RQU_Request(Guid server, Guid socket, string client, BINLPacket packet)
		{
			var OSChooserDir = new DirectoryInfo(Path.Combine(RootPath, "OSChooser"));
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
					if (Language.Name != "i386")
						oscml.AppendFormat($"<OPTION VALUE=\"{Language.Name.ToUpper()}\"> {Language.Name}");

				oscml.AppendLine("</SELECT>");
				oscml.AppendLine("</FORM>");
				oscml.AppendLine("</BODY>");
				oscml.AppendLine("</OSCML>");
			}
			else
			{
				var screen = Encoding.ASCII.GetString(packet.Data);

                var fileContent = File.ReadAllText(Path.Combine(OSChooserDir.FullName,
					string.IsNullOrEmpty(screen) ? "welcome.osc" : Path.Combine("english",
					string.Format($"{screen.ToLowerInvariant()}.osc"))));

				PrintMessage?.Invoke(this, new($"[I] OSChooser Screen Request: {screen.ToLowerInvariant()}"));

				fileContent = fileContent.Replace("\r\n", "\n");

				var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
				var hostname = Environment.MachineName;

				if (!domain.Contains('.') && !string.IsNullOrEmpty(domain))
					domain = string.Join(".", hostname, domain); // HOSTNAME.LOCALDOMAIN

				fileContent = fileContent.Replace("%MACHINEDOMAIN%", string.IsNullOrEmpty(domain) ? hostname : domain);
				oscml.Append(fileContent);
			}
			#endregion

			var response = new BINLPacket(ServiceType, BINLMessageTypes.ResponseUnsigned);
			response.Sequence = packet.Sequence;
			response.Fragment = packet.Fragment;
			response.TotalFragments = packet.TotalFragments;
			response.SignLength = packet.SignLength;
			response.Sign = packet.Sign;
			response.Data = Encoding.ASCII.GetBytes(oscml.ToString());
			response.Length = ((uint)response.Buffer.Length - 8);

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

		public void Heartbeat()
		{
		}

		public bool Initialize(XmlNode xmlConfigNode)
		{
			RootPath = Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot");
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
