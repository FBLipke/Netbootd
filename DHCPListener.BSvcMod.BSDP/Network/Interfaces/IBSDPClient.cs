using Netboot.Module.DHCPListener;
using System.Net;

namespace DHCPListener.BSvcMod.BSDP
{
	public interface IBSDPClient : IDHCPClient
	{		
		bool TestClient { get; set; }
	}
}
