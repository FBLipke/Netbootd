using Netboot.Common;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Netboot.Module.BINLServer
{
    public class BINLServerBase
    {
        private Filesystem Filesystem { get; set; }

        public static Dictionary<BootServerType, List<IBinlService>> Services { get; set; } = [];

        private IDatabase Database { get; set; }

        public delegate void BootServiceRequestEventHandler
            (object sender, BinlServiceRequestEventArgs e);

        public delegate void ListenerRequestReceivedEventHandler
            (object sender, ListenerRequestReceivedEventArgs e);

        delegate void RegisterBootServiceEventHandler
            (IBinlService sender, RegisterBinlServiceEventArgs e);

        public event ListenerRequestReceivedEventHandler
            ListenerRequestReceived;

        public static event BootServiceRequestEventHandler
            BinlServiceRequest;

        static event RegisterBootServiceEventHandler
            _RegisterBinlService;



        public static void RegisterBootService(IBinlService sender, BinlServerType serverType, string description, List<IPAddress> addresses = null)
        {
            _RegisterBinlService.Invoke(sender, new RegisterBinlServiceEventArgs(serverType, description, addresses));
        }

        public BINLServerBase(Filesystem filesystem, IDatabase database)
        {
            Database = database;
            Filesystem = filesystem;

            BinlServiceRequest += (sender, e) => {
            };

            ListenerRequestReceived += (sender, e) => {
                Thread.Sleep(1);
                BinlServiceRequest?.Invoke(this, new(e.Request, e.Server, e.Socket, e.Client));
            };

            _RegisterBinlService = (sender, e) => {
                if (Services.ContainsKey(e.Type))
                    Services[e.Type].Add(sender);
                else
                    Services.Add(sender.ServerType, [sender]);

                NetbootBase.Log("I", "BINLServer",
                    string.Format("Registered Service \"{0}\"", sender.ServerType));
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
            foreach (var service in Services.Values.ToList())
                foreach (var bs in service)
                    bs.HeartBeat();
        }

        public void Bootstrap(XmlNode xml)
        {
            #region "Load Service Modules"
            var serviceModules = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("BINLServer.SvcMod.*.dll", SearchOption.TopDirectoryOnly);

            foreach (var module in serviceModules.ToList())
            {
                var ass = Assembly.LoadFrom(module.FullName);

                var retvalColl = from t in ass.GetTypes()
                                 where (t.IsSubclassOf(typeof(IBinlService)) || t.GetInterfaces()
                                     .Contains(typeof(IBinlService))) && t.IsAbstract == false
                                 let moduleName = module.Name.Split('.')[2].Trim()
                                 select (t, moduleName);

                foreach (var (t, name) in retvalColl.ToList())
                {
                    try
                    {
                        var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance,
                            null, null, new object[] { xml }) as IBinlService;

                        if (b == null)
                            continue;

                        var bsType = b.ServerType;

                        if (!Services.ContainsKey(bsType))
                            Services.Add(bsType, [b]);
                        else
                            Services[bsType].Add(b);
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

        public void Handle_BootService_Request(Guid client, BINLPacket requestPacket)
        {
            throw new NotImplementedException();
        }

        public void Handle_BootService_Request(string client, BINLPacket requestPacket)
        {
            throw new NotImplementedException();
        }
    }
}
