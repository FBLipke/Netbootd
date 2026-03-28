// Decompiled with JetBrains decompiler
// Type: Netboot.Module.Eventlog
// Assembly: Netboot.Module.Eventlog, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB206447-7AFD-4668-A541-6AFE81129AE2
// Assembly location: C:\Users\LipkeGu\Desktop\Netboot___\Netboot.Module.Eventlog.dll

using Netboot.Common;
using Netboot.Common.Cryptography.Interfaces;
using Netboot.Common.Database;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.Network;
using Netboot.Common.Network.HTTP;
using Netboot.Common.Provider;
using Netboot.Common.Provider.Events;
using Netboot.Common.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Netboot.Module
{
	public class EventLog : IProvider, IManager, ILog
	{
		public Filesystem Filesystem { get; set; }

		public IDatabase? Database { get; set; }

		public Dictionary<Guid, IMember> Members { get; set; }

		public bool VolativeModule { get; set; } = true;

		public bool CanEdit { get; set; }

		public string FriendlyName { get; set; } = "Ereignisanzeige";

		public string Description { get; set; } = "Listet Meldungen welche zur Laufzeit auftreten auf.";

		public bool CanAdd { get; set; }

		public bool CanRemove { get; set; }

		public bool IsPublicModule { get; set; }

		public bool Active { get; set; } = false;
        public ICrypto Crypt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public EventLog()
		{
			CanAdd = false;
			CanEdit = false;
			CanRemove = false;
			Members = new Dictionary<Guid, IMember>();
			Filesystem = new Filesystem("Providers\\Eventlog");

			if (VolativeModule)
				return;
			Database = new SqlDatabase(Filesystem, "Eventlog.db");
		}

		public void Bootstrap(XmlNode xml)
		{
			if (VolativeModule)
				return;
			
			Database?.Bootstrap(xml);
		}

		public void Close()
		{
			if (VolativeModule)
				return;
			Database.Close();
		}

		public bool Contains(Guid id) => Members.ContainsKey(id);

		public void Dispose()
		{
			Database?.Dispose();
			Members?.Clear();
		}

		public string Handle_Get_Request(NetbootHttpContext request) => JsonConvert.SerializeObject(Members.Values);

		public void Install()
		{
		}

		public IMember? Get_Member(Guid id) => Members.TryGetValue(id, out IMember? value) ? value : null;

		public void HeartBeat()
		{
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
			NetbootBase.Log("I", FriendlyName, "Log messages will redirected to this Module!");
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
