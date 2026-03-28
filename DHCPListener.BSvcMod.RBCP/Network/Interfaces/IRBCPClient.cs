using Netboot.Module.DHCPListener;
using System.Net;

namespace DHCPListener.BSvcMod.RBCP
{
	public interface IRBCPClient : IDHCPClient
	{
		public ushort Layer { get; set; }

		public ushort Item { get; set; }
		
		bool TestClient { get; set; }
	}
}
