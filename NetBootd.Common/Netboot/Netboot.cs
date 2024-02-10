using Netboot.Common.Network.Sockets.Server;
using NetBoot.Common.Netboot.Common.Network.Definitions;
using NetBoot.Common.Netboot.Common.Network.Interfaces;
using System.Xml;

namespace NetBoot
{
	public class NetbootBase : IDisposable
	{
		public static Dictionary<Guid, IServer> Servers = [];
		public static Dictionary<Guid, IClient> Clients = [];

		string[] cmdArgs = [];

		public static string WorkingDirectory = Directory.GetCurrentDirectory();

		public NetbootBase(string[] args)
		{
			cmdArgs = args;
		}

		public bool Initialize()
		{
			Console.WriteLine("Netboot 0.1a ({0})", Functions.IsLittleEndian() ? "LE (LittleEndian)" : "BE (BigEndian)");

			var ConfigDir = Path.Combine(WorkingDirectory, "Config");
			if (!Directory.Exists(ConfigDir))
				Directory.CreateDirectory(ConfigDir);

			if (!Directory.Exists(Path.Combine(WorkingDirectory, "Dump")))
				Directory.CreateDirectory(Path.Combine(WorkingDirectory, "Dump"));

			var tftpRoot = Path.Combine(WorkingDirectory, "TFTPRoot");

			if (!Directory.Exists(Path.Combine(tftpRoot, "Boot")))
				Directory.CreateDirectory(Path.Combine(tftpRoot, "Boot"));

			if (!Directory.Exists(Path.Combine(tftpRoot, "Images")))
				Directory.CreateDirectory(Path.Combine(tftpRoot, "Images"));

			if (!Directory.Exists(Path.Combine(tftpRoot, "OSChooser")))
				Directory.CreateDirectory(Path.Combine(tftpRoot, "OSChooser"));

			#region Parse Config File
			var doc = new XmlDocument();
			doc.Load(Path.Combine(ConfigDir, "Netboot.xml"));

			{
				var serverEntries = doc.SelectNodes("Netboot/Configuration/Network/Server");
				if (serverEntries == null)
					return false;

				if (serverEntries.Count != 0)
				{
					foreach (XmlNode serverNode in serverEntries)
					{
						if (serverNode == null)
							continue;

						var port = serverNode.Attributes?.GetNamedItem("port")?.Value;
						if (string.IsNullOrEmpty(port))
						{
							Console.WriteLine("[E] Server: No Port given!");
							continue;
						}

						if (!ushort.TryParse(port, out var serverPort))
						{
							Console.WriteLine("[E] Server: Invalid port given!");
							continue;
						}


						var type = serverNode.Attributes?.GetNamedItem("type")?.Value;
						if (string.IsNullOrEmpty(type))
						{
							Console.WriteLine("[E] Server: No type given!");
							continue;
						}

						if (!Enum.TryParse<ServerType>(type.ToUpper(), out var serverType))
						{
							Console.WriteLine("[E] Server: Invalid Argument for type!");
							continue;
						}

						var serverId = Guid.NewGuid();
						Servers.Add(serverId, new BaseServer(serverId, serverType, serverPort));
					}
				}
			}
			#endregion

			return true;
		}

		public void Start()
		{
			foreach (var server in Servers)
				server.Value.Start();
		}

		public void Stop()
		{
			foreach (var server in Servers)
				server.Value.Stop();
		}

		public void Dispose()
		{
			foreach (var server in Servers)
				server.Value.Dispose();
		}
	}
}
