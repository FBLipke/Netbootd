using Netboot.Network.Server;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;
using Netboot.Services;
using System.Reflection;
using System.Xml;
using Netboot.Common;

namespace Netboot
{
	public class NetbootBase : IDisposable
	{
		public static Dictionary<Guid, IServer> Servers = [];
		public static Dictionary<string, IService> Services = [];

		string[] cmdArgs = [];

		public static string WorkingDirectory = Directory.GetCurrentDirectory();
		public static string ConfigFile = string.Empty;
		public static string BootServerRoot = Path.Combine(WorkingDirectory, "BootServer");

		public NetbootBase(string[] args)
		{
			cmdArgs = args;
		}

		void ParseArguments(string[] args)
		{
			foreach (var arg in args)
			{
				if (!arg.StartsWith("--"))
					continue;

				var kvPair = arg.Substring(2).Split(':', 1);

				switch (kvPair[0].ToLower())
				{
					case "root":
						if (string.IsNullOrEmpty(kvPair[1]))
							continue;

						var value = kvPair[1];
						if (value == "~")
							WorkingDirectory = Directory.GetCurrentDirectory();
						else
						{
							if (Directory.Exists(kvPair[1]))
								WorkingDirectory = kvPair[1];
						}
						break;
					case "config":
						if (string.IsNullOrEmpty(kvPair[1]))
							continue;

						var configValue = kvPair[1];
						if (configValue == "~")
							ConfigFile = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Netboot.xml"));
						else
						{
							if (!File.Exists(kvPair[1]))
							{
								Console.WriteLine($"Configfile: {configValue} not found! -> using default...");
								break;
							}

							ConfigFile = kvPair[1];
						}
						break;
					default:
						break;
				}
			}
		}

		public static void LoadServices()
		{
			Add_Service(new BaseService("NONE"));

			var serviceModules = new DirectoryInfo(WorkingDirectory)
				.GetFiles("Netboot.Service.*.dll", SearchOption.AllDirectories);

			foreach (var module in serviceModules)
			{
				var ass = Assembly.LoadFrom(module.FullName);
				foreach (var (t, serviceType) in from t in ass.GetTypes()
												 where (t.IsSubclassOf(typeof(IService)) || t.GetInterfaces().Contains(typeof(IService))) && t.IsAbstract == false
												 let serviceType = module.Name.Split('.')[2].Trim().ToUpper()
												 select (t, serviceType))
				{
					try
					{
						var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance,
							null, null, new[] { serviceType }) as IService;
						Add_Service(b);

					}
					catch (MissingMethodException ex)
					{
						Console.WriteLine(ex.Message);
						throw;
					}
				}
			}
		}

		public static void Add_Service(IService service)
		{
			service.AddServer += (sender, e) =>
			{
				Add_Server(e.ServiceType, e.Ports);
			};

			service.ServerSendPacket += (sender, e) =>
			{
				Servers[e.ServerId].Send(e.SocketId, e.Packet, e.Client);
			};

			Services.Add(service.ServiceType, service);

			Console.WriteLine($"[I] Added Service for '{service.ServiceType}'");
		}

		public bool Initialize()
		{
			Console.WriteLine("Netboot 0.1a ({0})", Functions.IsLittleEndian()
				? "LE (LittleEndian)" : "BE (BigEndian)");

			ConfigFile = Path.Combine(WorkingDirectory, "Config", "Netboot.xml");

			if (!File.Exists(ConfigFile))
				throw new FileNotFoundException(ConfigFile);

			LoadServices();


			MD4 md4 = new MD4();
			md4.Initialize();
			md4.ComputeHash(System.Text.Encoding.ASCII.GetBytes("Administrator"));
			Console.WriteLine("REF: 716f3dcab5f869c18ded1ddf987a276a");
			Console.WriteLine(string.Join("",md4.Hash.Select(x => x.ToString("X2"))));
			#region "Read Config File"
			var xmlFile = new XmlDocument();
			xmlFile.Load(ConfigFile);

			var services = xmlFile.SelectNodes("Netboot/Configuration/Services/Service");
			foreach (var service in Services.Values)
			{
				foreach (XmlNode xmlnode in services)
				{
					var node = xmlnode.Attributes.GetNamedItem("type");
					if (node.Value != service.ServiceType.ToLower())
						continue;

					service.Initialize(xmlnode);
				}
			}

			#endregion
			
			return true;
		}

		public void Start()
		{
			foreach (var service in Services.Values.ToList())
				service.Start();

			foreach (var server in Servers.Values.ToList())
				server.Start();
		}

		public void Heartbeat()
		{
			foreach (var service in Services.Values.ToList())
				service.Heartbeat();
		}

		public static void Add_Server(string serviceType, IEnumerable<ushort> ports)
		{
			var serverId = Guid.NewGuid();
			var server = new BaseServer(serverId, serviceType, ports);
			server.DataSent += (sender, e) =>
			{
				Functions.InvokeMethod(Services[e.ServiceType], "Handle_DataSent",
					new[] { new[] { sender, e } });
			};

			server.DataReceived += (sender, e) =>
			{
				try
				{
					// Microsoft BINL (RIS) uses also port 4011. So differentiate between BINL and BOOTP (/ DHCP)
					if (e.Packet[0] > 2 && e.ServiceType == "DHCP")
						Functions.InvokeMethod(Services["BINL"], "Handle_DataReceived", new[] { sender, e });
					else
						Functions.InvokeMethod(Services[e.ServiceType], "Handle_DataReceived", new[] { sender, e });
				}
				catch (KeyNotFoundException ex)
				{
					Console.WriteLine($"[E] Cant find Service for '{e.ServiceType}'");
				}
			};

			Servers.Add(serverId, server);
		}

		public void Stop()
		{
			foreach (var service in Services.Values.ToList())
				service.Stop();

			foreach (var server in Servers.Values.ToList())
				server.Stop();
		}

		public void Dispose()
		{
			foreach (var service in Services.Values.ToList())
				service.Dispose();

			foreach (var server in Servers.Values.ToList())
				server.Dispose();
		}
	}
}
