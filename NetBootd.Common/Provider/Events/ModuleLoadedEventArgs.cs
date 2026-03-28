// Decompiled with JetBrains decompiler
// Type: Netboot.Common.Provider.ModuleLoadedEventArgs
// Assembly: Netboot.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CE4FCADF-C52D-4962-B4B8-C6D36FAB8FAE
// Assembly location: C:\Users\LipkeGu\Desktop\Netboot___\Netboot.Common.dll

using System.Xml;

namespace Netboot.Common.Provider.Events
{
	public class ModuleLoadedEventArgs
	{
		public IProvider Module { get; private set; }

		public string Name { get; private set; }

		public XmlNodeList Xml { get; private set; }

		public ModuleLoadedEventArgs(IProvider module, string name, XmlNodeList xml)
		{
			Xml = xml;
			Name = name;
			Module = module;
		}
	}
}
