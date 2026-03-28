using Netboot.Common;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.System;
using Netboot.Module.DHCPListener.Event;
using System.Net;
using System.Reflection;
using System.Xml;

namespace Netboot.Module.DHCPListener
{
	public class DHCPListenerBase : IManager, IBootService
	{
		private Dictionary<string, IDHCPClient> Clients { get; set; } = [];

		private Filesystem Filesystem { get; set; }

		public static Dictionary<BootServerType, List<IBootService>> Bootservices { get; set; } = [];

		private IDatabase Database { get; set; }

		public BootServerType ServerType { get; set; } = BootServerType.PXEBootstrapServer;

		public delegate void BootServiceRequestEventHandler
			(object sender, BootServiceRequestEventArgs e);

		public delegate void ListenerRequestReceivedEventHandler
			(object sender, ListenerRequestReceivedEventArgs e);

		delegate void RegisterBootServiceEventHandler
			(IBootService sender, RegisterBootServiceEventArgs e);

		public event ListenerRequestReceivedEventHandler
			ListenerRequestReceived;

		public static event BootServiceRequestEventHandler
			BootServiceRequest;

		static event RegisterBootServiceEventHandler
			_RegisterBootService;



		public static void RegisterBootService(IBootService sender, BootServerType bootServerType, string description, List<IPAddress> addresses = null)
		{
			_RegisterBootService.Invoke(sender, new RegisterBootServiceEventArgs(bootServerType,description, addresses));
		}

		public DHCPListenerBase(Filesystem filesystem, IDatabase database)
		{
			Database = database;
			Filesystem = filesystem;

            BootServiceRequest += (sender, e) => {
            };

            ListenerRequestReceived += (sender, e) => {
                Thread.Sleep(1);
                BootServiceRequest?.Invoke(this, new BootServiceRequestEventArgs(e.Request, e.Server, e.Socket, e.Client));
            };

            _RegisterBootService = (sender, e) => {
                if (Bootservices.ContainsKey(e.Type))
                    Bootservices[e.Type].Add(sender);
                else
                    Bootservices.Add(sender.ServerType, [sender]);

                NetbootBase.Log("I", "DHCPListener",
                    string.Format("Registered BootService \"{0}\"", sender.ServerType));
            };
        }

		public void Start()
		{
		}

		public void Stop()
		{
		}

		public void HeartBeat()
		{
			foreach (var bootservices in Bootservices.Values.ToList())
				foreach (var bs in bootservices)
					bs.HeartBeat();
		}

		public void Bootstrap(XmlNode xml)
		{
			#region "Load Service Modules"
			var serviceModules = new DirectoryInfo(Directory.GetCurrentDirectory())
				.GetFiles("DHCPListener.BSvcMod.*.dll", SearchOption.TopDirectoryOnly);

			foreach (var module in serviceModules.ToList())
			{
				var ass = Assembly.LoadFrom(module.FullName);

				var retvalColl = from t in ass.GetTypes()
								 where (t.IsSubclassOf(typeof(IBootService)) || t.GetInterfaces()
									 .Contains(typeof(IBootService))) && t.IsAbstract == false
								 let moduleName = module.Name.Split('.')[2].Trim()
								 select (t, moduleName);

				foreach (var (t, name) in retvalColl.ToList())
				{
					try
					{
                        var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance,
							null, null, new object[] { xml }) as IBootService;

						if (b == null)
							continue;

						var bsType = b.ServerType;

						if (!Bootservices.ContainsKey(bsType))
							Bootservices.Add(bsType, [b]);
						else
							Bootservices[bsType].Add(b);
					}
					catch (MissingMethodException ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
			}
			#endregion
		}

		public void Close()
		{
		}

		public void Dispose()
		{
		}
		public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
		{
			ListenerRequestReceived?.Invoke(this, new ListenerRequestReceivedEventArgs(memoryStream, server, socket, client));
		}

		public void Handle_BootService_Request(Guid client, DHCPPacket requestPacket)
		{
			throw new NotImplementedException();
		}

		public void Handle_BootService_Request(string client, DHCPPacket requestPacket)
		{
			throw new NotImplementedException();
		}
	}
}
