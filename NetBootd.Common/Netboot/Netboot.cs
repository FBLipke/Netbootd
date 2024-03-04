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
using Netboot.Network.Interfaces;
using Netboot.Network.Server;
using Netboot.Services;
using Netboot.Services.Interfaces;
using System.Reflection;
using System.Xml;

namespace Netboot
{
    public class NetbootBase : IDisposable
	{
		public static Dictionary<Guid, IServer> Servers = [];
		public static Dictionary<string, IService> Services = [];
        private string[] cmdArgs = [];

		public static NetbootPlatform Platform = new NetbootPlatform();

		public NetbootBase(string[] args)
		{
			cmdArgs = args;
		}

		public static void LoadServices()
		{
			Add_Service(new BaseService("NONE"));

            #region "Load Service Modules"
            var serviceModules = new DirectoryInfo(Platform.NetbootDirectory)
				.GetFiles("Netboot.Service.*.dll", SearchOption.TopDirectoryOnly);

			foreach (var module in serviceModules)
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
						
						Add_Service(b);

					}
					catch (MissingMethodException ex)
					{
						Console.WriteLine(ex.Message);
						throw;
					}
				}
			}
            #endregion
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

			Platform.Initialize();

			var ConfigFile = Path.Combine(Platform.ConfigDirectory, "Netboot.xml");

			if (!File.Exists(ConfigFile))
				throw new FileNotFoundException(ConfigFile);

			LoadServices();

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
