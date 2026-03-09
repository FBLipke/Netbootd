using Netboot.Module.DHCPListener;
using static DHCPListener.BSvcMod.RBCP.Definitions.Definitions;

namespace DHCPListener.BSvcMod.RBCP.Definitions
{
	public class BootItem
	{
		public BootServerType Item;
		public RBCPLayer Layer;

		public BootItem(BootServerType item, RBCPLayer layer)
		{
			Item = item;
			Layer = layer;
		}
	}
}
