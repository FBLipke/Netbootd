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
using Netboot.Common.System;
using System.Reflection;
using System.Xml;

namespace Netboot.Module.BINLListener
{
    public class BINLListenerBase : IManager
    {
        public static Dictionary<BINLMessageTypes, List<IBINLService>> BINLServices { get; set; } = [];

        public delegate void BINLServiceRequestEventHandler(object sender, BINLServiceRequestEventArgs e);
        public delegate void ListenerRequestReceivedEventHandler(object sender, BINLListenerRequestReceivedEventArgs e);
        delegate void RegisterBINLServiceEventHandler(IBINLService sender, RegisterBINLServiceEventArgs e);

        public event BINLServiceRequestEventHandler? BINLServiceRequest;
        public event ListenerRequestReceivedEventHandler? ListenerRequestReceived;

        static event RegisterBINLServiceEventHandler? _RegisterBINLService;

        public static void RegisterBINLService(IBINLService sender, BINLMessageTypes messageType, string description)
        {
            _RegisterBINLService?.Invoke(sender, new RegisterBINLServiceEventArgs(messageType, description));
        }

        public BINLListenerBase()
        {
            BINLServiceRequest += (sender, e) => { };
            ListenerRequestReceived += (sender, e) =>
            {
                Thread.Sleep(1);
                var binlPacket = new BINLPacket(e.Request.GetBuffer());
                BINLServiceRequest?.Invoke(this, new BINLServiceRequestEventArgs(binlPacket, e.Server, e.Socket, e.Client));
            };
            _RegisterBINLService = (sender, e) =>
            {
                if (BINLServices.ContainsKey(e.MessageType))
                    BINLServices[e.MessageType].Add(sender);
                else
                    BINLServices.Add(e.MessageType, [sender]);
                NetbootBase.Log("I", "BINLListener", string.Format("Registered BINL Service \"{0}\" for {1}", sender.GetType().Name, e.MessageType));
            };
        }

        public void Start() { }
        public void Stop() { }

        public void HeartBeat()
        {
            foreach (var services in BINLServices.Values.ToList())
                foreach (var service in services)
                    service.HeartBeat();
        }

        public void Bootstrap(XmlNode xml)
        {
            var serviceModules = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("BINLListener.BSvcMod.*.dll", SearchOption.TopDirectoryOnly);

            foreach (var module in serviceModules.ToList())
            {
                var ass = Assembly.LoadFrom(module.FullName);
                var retvalColl = from t in ass.GetTypes()
                                  where (t.IsSubclassOf(typeof(IBINLService)) || t.GetInterfaces().Contains(typeof(IBINLService))) && t.IsAbstract == false
                                  let moduleName = module.Name.Split('.')[2].Trim()
                                  select (t, moduleName);

                foreach (var (t, name) in retvalColl.ToList())
                {
                    try
                    {
                        var b = t.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[] { xml }) as IBINLService;
                        if (b == null) continue;
                        var binlType = b.MessageType;
                        if (!BINLServices.ContainsKey(binlType))
                            BINLServices.Add(binlType, [b]);
                        else
                            BINLServices[binlType].Add(b);
                    }
                    catch (MissingMethodException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public void Close() { }
        public void Dispose() { }

        public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
        {
            ListenerRequestReceived?.Invoke(this, new BINLListenerRequestReceivedEventArgs(memoryStream, server, socket, client));
        }

        public void Handle_BINL_Service_Request(Guid client, BINLPacket requestPacket)
        {
            throw new NotImplementedException();
        }

        public void Handle_BINL_Service_Request(string client, BINLPacket requestPacket)
        {
            throw new NotImplementedException();
        }
    }

    public class BINLServiceRequestEventArgs : EventArgs
    {
        public BINLPacket Request { get; set; }
        public Guid Server { get; set; }
        public Guid Socket { get; set; }
        public Guid Client { get; set; }

        public BINLServiceRequestEventArgs(BINLPacket request, Guid server, Guid socket, Guid client)
        {
            Request = request;
            Server = server;
            Socket = socket;
            Client = client;
        }
    }

    public class BINLListenerRequestReceivedEventArgs : EventArgs
    {
        public MemoryStream Request { get; set; }
        public Guid Server { get; set; }
        public Guid Socket { get; set; }
        public Guid Client { get; set; }

        public BINLListenerRequestReceivedEventArgs(MemoryStream request, Guid server, Guid socket, Guid client)
        {
            Request = request;
            Server = server;
            Socket = socket;
            Client = client;
        }
    }

    public class RegisterBINLServiceEventArgs : EventArgs
    {
        public BINLMessageTypes MessageType { get; set; }
        public string Description { get; set; }

        public RegisterBINLServiceEventArgs(BINLMessageTypes messageType, string description)
        {
            MessageType = messageType;
            Description = description;
        }
    }
}