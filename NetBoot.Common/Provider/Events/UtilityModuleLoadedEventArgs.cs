using Netboot.Common.Provider.Events;
using Netboot.Common.Utility;
using System.Xml;

namespace Netboot.Common.Provider
{
	public class UtilityModuleLoadedEventArgs
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public IUtility Module { get; }

		public Guid Id { get; set; }

		public UtilityModuleLoadedEventArgs(IUtility module, string name, XmlNodeList xml)
		{
			Module = module;
			Id = Guid.NewGuid();
			Name = name;
		}
	}
}