using Netboot.Common;
using Netboot.Common.Cryptography.Interfaces;
using Netboot.Common.Database;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.Network.HTTP;
using Netboot.Common.Network.sockets;
using Netboot.Common.Provider;
using Netboot.Common.Provider.Events;
using Netboot.Common.System;
using Newtonsoft.Json;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace Netboot.Module.DHCPListener
{
	public class DHCPListener : IProvider, IManager, ILog
	{
		public Filesystem Filesystem { get; set; }

		public IDatabase Database { get; set; }

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


		public void Bootstrap()
		{
			NetbootBase.NetworkManager.ServerManager.Add(ProtoType.Udp, ServerMode.Udp, [67, 4011]);

			NetbootBase.NetworkManager.UDPRequestReceived += (sender, e) => {
				Base.Handle_Listener_Request(e.Server, e.Socket, e.Client,e.Data);
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
			Base.Dispose();

			if (Database != null)
			{
				Database.Dispose();
				Database = null;
			}
			if (Members != null)
			{
				Members.Clear();
				Members = null;
			}
			
			Filesystem = null;
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

		public void Log(string type, string name, string logmessage)
		{
			var str = "\t" + DateTime.Now.ToString("dd.MM.yyyy : HH:mm:ss", CultureInfo.InvariantCulture)
				+ "\tNetboot." + name + ": " + logmessage;

			var key = Guid.NewGuid();
			var num = DateTime.Now.AsUnixTimeStamp();

			Members.Add(key, new Member()
			{
				Name = name,
				Id = key,
				Description = str,
				Author = Guid.Empty,
				Created = num,
				Updated = num,
				Provider = name,
				Url = "-"
			});

			Console.WriteLine("[" + type + "]" + str);
		}

		public string Handle_Redirect_Request(bool loggedin, string redirectTo, string content = "")
		{
			throw new NotImplementedException();
		}
	}
}
