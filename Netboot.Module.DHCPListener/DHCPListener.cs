using Netboot.Common;
using Netboot.Common.Cryptography.Interfaces;
using Netboot.Common.Database;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.Network.HTTP;
using Netboot.Common.Network.Sockets;
using Netboot.Common.Provider;
using Netboot.Common.Provider.Events;
using Netboot.Common.System;
using Newtonsoft.Json;
using System.Net;
using System.Xml;

namespace Netboot.Module.DHCPListener
{
    public class DHCPListener : IProvider, IManager
    {
        public Filesystem Filesystem { get; set; }

        public IDatabase Database { get; set; }

        public Guid Server { get; private set; }
        public Dictionary<Guid, IMember> Members { get; set; }

        public bool VolativeModule { get; set; } = false;

        public bool CanEdit { get; set; }

        public string FriendlyName { get; set; } = "DHCPListener";

        public string Description { get; set; } = "";

        public bool CanAdd { get; set; }

        public bool CanRemove { get; set; }

        public bool IsPublicModule { get; set; }

        public bool Active { get; set; } = false;

        public ICrypto Crypt { get; set; }

        private DHCPListenerBase Base { get; set; }


        public DHCPListener()
        {
            CanAdd = false;
            CanEdit = false;
            CanRemove = false;
            Members = [];
            Filesystem = new Filesystem("Providers\\DHCPListener");
            Database = new SqlDatabase(Filesystem, "DHCPListener.db");
            Base = new DHCPListenerBase(Filesystem, Database);
        }


        public void Bootstrap(XmlNode xml)
        {
            var _ports = new List<ushort>();

            var ports = xml.Attributes.GetNamedItem("port").Value.Split(',').ToList();
            if (ports.Count > 0)
                _ports.AddRange(from port in ports select ushort.Parse(port.Trim()));

            var mcastAddress = IPAddress.Parse(xml.Attributes.GetNamedItem("mcaddr").Value);

            Server = NetbootBase.NetworkManager.ServerManager.Add(ProtoType.Udp, _ports);
            NetbootBase.NetworkManager.ServerManager.JoinMulticastGroup(Server, mcastAddress);

            NetbootBase.NetworkManager.UDPRequestReceived += (sender, e) =>
            {
                Base.Handle_Listener_Request(e.Server, e.Socket, e.Client, e.Data);
            };

            Base.Bootstrap(xml);

            if (VolativeModule)
                return;

            Database?.Bootstrap(xml);
        }

        public void Close()
        {

            Base.Close();

            if (VolativeModule)
                return;

            Database.Close();
        }

        public bool Contains(Guid id) => Members.ContainsKey(id);

        public void Dispose()
        {
            Base.Dispose();

            if (Database != null)
                Database.Dispose();

            if (Members != null)
                Members.Clear();

            Filesystem?.Dispose();
        }

        public string Handle_Get_Request(NetbootHttpContext request) => JsonConvert.SerializeObject(Members.Values);

        public void Install()
        {
        }

        public IMember Get_Member(Guid id)
            => Members.ContainsKey(id) ? Members[id] : null;

        public void HeartBeat()
        {
            Base.HeartBeat();

            if (VolativeModule)
                return;
            Database?.HeartBeat();
            Update();
        }

        public void Remove(Guid id) => Members.Remove(id);

        public IMember Request(Guid id) => Members[id];

        public void Start()
        {
            Active = true;
            NetbootBase.Log("I", FriendlyName, "This module filters BOOTP and BINL Messages");
            Base.Start();
        }

        public void Stop()
        {
            Active = false;
            Base.Stop();

            Provider.Commit(Members, FriendlyName, Database, Filesystem);
        }

        public void Update()
        {
            if (!Active)
                return;

            Provider.Commit(Members, FriendlyName, Database, Filesystem);
        }

        public string Handle_Add_Request(NetbootHttpContext context)
        {
            throw new NotImplementedException();
        }

        public string Handle_Edit_Request(NetbootHttpContext context)
        {
            throw new NotImplementedException();
        }

        public string Handle_Remove_Request(NetbootHttpContext context)
        {
            throw new NotImplementedException();
        }

        public string Handle_Info_Request(NetbootHttpContext context)
        {
            throw new NotImplementedException();
        }

        public string Handle_Redirect_Request(bool loggedin, string redirectTo, string content = "")
        {
            throw new NotImplementedException();
        }
    }
}
