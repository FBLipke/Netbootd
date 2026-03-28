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

using Netboot.Common.Common;
using Netboot.Common.Network;
using Netboot.Common.Provider;
using Netboot.Common.Provider.Events;
using Netboot.Common.System;
using System.Reflection;
using System.Xml;

namespace Netboot.Common
{
	public class NetbootBase : IDisposable, IManager
	{
		private Thread _heartBeatThread;

		public static NetworkManager NetworkManager { get; private set; }

		public static Dictionary<string, IProvider>? Providers { get; private set; }

		public Filesystem FileSystem { get; set; }

		private string[] cmdArgs = [];

		public bool Running { get; private set; }

		public static NetbootPlatform Platform = new();

		public NetbootBase(string[] args)
		{
			var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
			var title = string.Format("NetBoot {0}.{1}", appVersion.Major, appVersion.Minor);
			Console.Title = title;

			cmdArgs = args;

			FileSystem = new Filesystem(Environment.CurrentDirectory);
			FileSystem.CreateDirectory("Config");


			_heartBeatThread = new Thread(new ThreadStart(HeartBeat));

			Providers = [];

            #region "Read Config File"
            #endregion

            Provider.Provider.ModuleLoaded += (sender, e) =>
			{
                Log("I", "Common", string.Format("Loading Module \"{0}\"...", e.Module));
                Providers.Add(e.Name, e.Module);
                var xmlFile = new XmlDocument();
                xmlFile.Load(Path.Combine(FileSystem.Root, "Config", "Netboot.xml"));
                var services = xmlFile.SelectNodes("Netboot/Configuration/Services/Service");

                foreach (XmlNode xmlnode in services)
                    if (e.Name == xmlnode.Attributes.GetNamedItem("type").Value)
						Providers[e.Name]?.Bootstrap(xmlnode);

                var funcs = new List<string>
                {
                    "Install",
                    "Start",
                    "HeartBeat"
                };

                foreach (var item in funcs)
                {
                    Log("I", "Common", string.Format("Sending \"{1}\" command  to \"{0}\"", e.Name, item));
                    Provider.Provider.InvokeMethod<IProvider>(Providers[e.Name], item, new object[] {});
                }
			};
			Task.Run(() =>
			{
				Provider.Provider.LoadModule(Directory.GetCurrentDirectory());
			});

			NetworkManager = new NetworkManager();
		}

		/*
		public static void LoadServices()
		{
			Add_Service(new BaseService("NONE", SocketProtocol.NONE));

			#region "Load Service Modules"
			var serviceModules = new DirectoryInfo(Platform.NetbootDirectory)
				.GetFiles("Netboot.Service.*.dll", SearchOption.TopDirectoryOnly);

			foreach (var module in serviceModules.ToList())
			{
				var ass = Assembly.LoadFrom(module.FullName);

				foreach (var (t, serviceType) in from t in ass.GetTypes()
					where (t.IsSubclassOf(typeof(IService)) || t.GetInterfaces()
						.Contains(typeof(IService))) && t.IsAbstract == false
							let serviceType = module.Name.Split('.')[2].Trim().ToUpper()
								select (t, serviceType))
				{
					try
					{
						var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance,
							null, null, new[] { serviceType }) as IService;

						if (b == null)
							continue;

						Add_Service(b);

					}
					catch (MissingMethodException ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
			}

			if (Services.Count == 0)
			{
				Console.WriteLine("[W] There is no service");
			}
			#endregion
		}

		
		public static void Add_Service(IService service)
		{
			service.AddServer += (sender, e) => {
				Add_Server(e.ServiceType, e.Protocol, e.Ports);
			};

			service.ServerSendPacket += (sender, e) => {
				Servers[e.ServerId].Send(e.SocketId, e.Packet, e.Client);
			};

			service.PrintMessage += (sender, e) => {
				Console.WriteLine(e.Message);
			};

			Services.Add(service.ServiceType, service);

            Console.WriteLine($"[I] Added Service for '{service.ServiceType}'");
		}

		
		public void Setup(XmlNode xmlConfigNode)
		{
			foreach (var service in Services.Values.ToList())
				service.Setup(xmlConfigNode);
		}

		
		public bool Initialize()
		{



			if (!Platform.Initialize())
			{
				Console.WriteLine("[E] Failed to initialize Platform.");
				return false;
			}

			var ConfigFile = Path.Combine(Platform.ConfigDirectory, "Netboot.xml");

			if (!File.Exists(ConfigFile))
				throw new FileNotFoundException(ConfigFile);

			LoadServices();



			foreach (var server in Servers.Values.ToList())
				server.Initialize();

			return true;
		}



		public static void Add_Server(string serviceType, SocketProtocol protocol, IEnumerable<ushort> ports)
		{
			var serverId = Guid.NewGuid();
			var server = new BaseServer(serverId, serviceType, protocol, ports);
			server.DataSent += (sender, e) =>
			{
				Functions.InvokeMethod(Services[e.ServiceType], "Handle_DataSent",
					[new[] { sender, e }]);
			};

			server.DataReceived += (sender, e) =>
			{
				try
				{
					var serviceType = e.Packet[0] > 2 && e.ServiceType == "DHCP" ? "BINL" : e.ServiceType;

					// Microsoft BINL (RIS) uses also port 4011. So differentiate between BINL and BOOTP (/ DHCP)
					Functions.InvokeMethod(Services[serviceType], "Handle_DataReceived", [sender, e]);
				}
				catch (KeyNotFoundException)
				{
					Console.WriteLine($"[E] Cant find Service for '{e.ServiceType}'");
				}
			};

			Servers.Add(serverId, server);
		}
		*/

		public void Start()
		{
			NetworkManager.Start();

			Running = Providers.Count != 0;
			_heartBeatThread.Start();
		}

		public void Stop()
		{
			NetworkManager.Stop();

			foreach (var provider in Providers)
			{
				Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Stop");
				Log("I", provider.Key, "stopped!");
			}
		}

		public void Dispose()
		{
			NetworkManager.Dispose();

			if (Providers.Count != 0)
			{
				foreach (var provider in Providers)
					Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Dispose");

				Providers.Clear();
				Providers = null;
			}

			try
			{
				_heartBeatThread.Abort();
			}
			catch
			{
			}

		}

		public void Bootstrap(XmlNode xml)
		{
            NetworkManager.Bootstrap(xml);

            foreach (var provider in Providers)
            {
                Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Bootstrap", new object[] { xml });
                Log("I", provider.Key, "closed!");
            }
        }

		public void Close()
		{
			NetworkManager.Close();

			foreach (var provider in Providers)
			{
				Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Close");
				Log("I", provider.Key, "closed!");
			}
		}

		public void HeartBeat()
		{
            Thread.Sleep(30000);
            NetworkManager.HeartBeat();

			foreach (var provider in Providers)
				Provider.Provider.InvokeMethod<IProvider>(provider.Value, "HeartBeat");
		}

		public static void Log(string type, string name, string logmessage)
		{
			if (!Provider.Provider.CanDo("Log").Any())
				Console.WriteLine(logmessage);
			else
				Provider.Provider.InvokeMethod(Provider.Provider.CanDo("Log").First(), "Log",
					[type, name, logmessage]);

		}
	}
}
