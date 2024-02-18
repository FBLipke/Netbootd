using Netboot.Network.Client;
using Netboot.Network.Definitions;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
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

		public string RootPath { get; set; }

		public string ServiceType { get; }

		public Dictionary<string, IClient> Clients { get; set; } = [];

		public event IService.AddServerEventHandler? AddServer;
		public event IService.ServerSendPacketEventHandler? ServerSendPacket;

		public void Dispose()
		{
		}

		public void Handle_RQU_Request(Guid server, Guid socket, string client, BINLPacket packet)
		{
			var OSChooserDir = new DirectoryInfo(Path.Combine(RootPath, "OSChooser"));
			var directories = OSChooserDir.GetDirectories();

			#region "Get the OSCML screen"
			var oscml = new StringBuilder("");

			// We may have more than one OSes with different languages on the server.
			if (directories.Count() > 1)
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
					oscml.AppendFormat($"<OPTION VALUE=\"{Language.Name.ToUpper()}\"> {Language.Name}");

				oscml.AppendLine("</SELECT>");
				oscml.AppendLine("</FORM>");
				oscml.AppendLine("</BODY>");
				oscml.AppendLine("</OSCML>");
			}
			else
			{
				var x = IPGlobalProperties.GetIPGlobalProperties().DomainName;
					Console.WriteLine(x);
				var screen = System.Text.Encoding.ASCII.GetString(packet.Data);

				var fileContent = File.ReadAllText(Path.Combine(OSChooserDir.FullName, "english",
					string.IsNullOrEmpty(screen) ? "welcome.osc" : string.Format($"{screen.ToLowerInvariant()}.osc")));
				
				fileContent = fileContent.Replace("\r\n", "\n");
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

		}
		void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
		{
			if (!Clients.ContainsKey(clientId))
				Clients.Add(clientId, new BINLClient(clientId, serviceType, remoteEndpoint, serverId, socketId));
			else
			{
				Clients[clientId].RemoteEntpoint = remoteEndpoint;
			}
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			var request = new BINLPacket(ServiceType, e.Packet);
			var clientid = e.RemoteEndpoint.Address.ToString();

			AddClient(clientid,ServiceType,e.RemoteEndpoint,e.ServerId,e.SocketId);

			switch (request.MessageType)
			{
				case BINLMessageTypes.RequestUnsigned:
					Handle_RQU_Request(e.ServerId, e.SocketId, clientid, request);
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
			return false;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}
	}
}
