using Netboot.Network.Server;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;
using Netboot.Services;
using System.Reflection;

namespace Netboot
{
	public class NetbootBase : IDisposable
	{
		public static Dictionary<Guid, IServer> Servers = [];
		public static Dictionary<string, IService> Services = [];

		string[] cmdArgs = [];

		public static string WorkingDirectory = Directory.GetCurrentDirectory();

		public NetbootBase(string[] args)
		{
			cmdArgs = args;
		}

		public static void LoadServices()
		{
			Add_Service(new BaseService("NONE"));

			var serviceModules = new DirectoryInfo(WorkingDirectory)
				.GetFiles("Netboot.Service.*.dll", SearchOption.AllDirectories);

			foreach (var module in serviceModules)
			{
				var ass = Assembly.LoadFrom(module.FullName);
				foreach (var t in ass.GetTypes())
				{
					if ((t.IsSubclassOf(typeof(IService)) || t.GetInterfaces().Contains(typeof(IService))) && t.IsAbstract == false)
					{
						var serviceType = module.Name.Split('.')[2].Trim().ToUpper();
						try
						{
							var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[] { serviceType }) as IService;
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

			LoadServices();

			foreach (var service in Services.Values)
				service.Initialize();

			return true;
		}

		public void Start()
		{
			foreach (var service in Services.Values)
				service.Start();

			foreach (var server in Servers)
				server.Value.Start();
		}

		public static void Add_Server(string serviceType, IEnumerable<ushort> ports)
		{
			var serverId = Guid.NewGuid();
			var server = new BaseServer(serverId, serviceType, ports);
			server.DataSent += (sender, e) =>
			{
				Functions.InvokeMethod(Services[e.ServiceType], "Handle_DataSent",
					new object[] { new object[] { sender, e } });
			};

			server.DataReceived += (sender, e) =>
			{
				try
				{
					// Microsoft BINL (RIS) uses also port 4011. So differentiate between BINL and BOOTP (/ DHCP)
					if (e.Packet[0] > 2 && e.ServiceType == "DHCP")
						Functions.InvokeMethod(Services["BINL"], "Handle_DataReceived", new object[] { sender, e });
					else
						Functions.InvokeMethod(Services[e.ServiceType], "Handle_DataReceived", new object[] { sender, e });
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
			foreach (var service in Services.Values)
				service.Stop();

			foreach (var server in Servers)
				server.Value.Stop();
		}

		public void Dispose()
		{
			foreach (var service in Services.Values)
				service.Dispose();

			foreach (var server in Servers)
				server.Value.Dispose();
		}
	}
}
