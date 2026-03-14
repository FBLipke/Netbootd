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

namespace Netboot.Module.TFTPServer
{
    public class TFTPServer : IProvider, IManager
    {
        public Filesystem Filesystem { get; set; }

        public IDatabase Database { get; set; }

        public Guid Server { get; private set; }

        public Dictionary<Guid, IMember> Members { get; set; }

        public bool VolativeModule { get; set; } = false;

        public bool CanEdit { get; set; }

        public string FriendlyName { get; set; } = "TFTPServer";

        public string Description { get; set; } = "";

        public bool CanAdd { get; set; }

        public bool CanRemove { get; set; }

        public bool IsPublicModule { get; set; }

        public bool Active { get; set; } = false;

        public ICrypto Crypt { get; set; }

        TFTPServerBase Base { get; set; }

        public TFTPServer()
        {
            CanAdd = false;
            CanEdit = false;
            CanRemove = false;
            Members = [];
            Filesystem = new Filesystem("Providers\\TFTPServer");
            Database = new SqlDatabase(Filesystem, "TFTPServer.db");
            Base = new TFTPServerBase(Filesystem, Database);
        }

        public void Bootstrap()
        {
            Server = NetbootBase.NetworkManager.ServerManager.Add(ProtoType.Udp, [69, 1758]);

            NetbootBase.NetworkManager.UDPRequestReceived += (sender, e) => {
                Base.Handle_Listener_Request(e.Server, e.Socket, e.Client, e.Data);
            };

            Base.Bootstrap();

            if (VolativeModule)
                return;

            Database?.Bootstrap();
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
            if (Database != null)
                Database.Dispose();

            if (Members != null)
                Members.Clear();

            Base.Dispose();
        }

        public string Handle_Get_Request(NetbootHttpContext request)
            => JsonConvert.SerializeObject(Members.Values);

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
            NetbootBase.Log("I", FriendlyName, "This module provides an basic TFTP Server");
        }

        public void Stop()
        {
            Active = false;

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
