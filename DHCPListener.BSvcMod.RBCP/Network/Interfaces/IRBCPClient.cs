using Netboot.Module.DHCPListener;
using System.Net;

namespace DHCPListener.BSvcMod.RBCP
{
	public interface IRBCPClient : IDHCPClient
	{
		public ushort Layer { get; set; }

		public ushort Item { get; set; }

		IPAddress McastDiscoveryAddress { get; set; }
		
		ushort McastClientPort { get; set; }

		ushort McastServerPort { get; set; }

		byte DiscoveryControl { get; set; }

		byte MulticastTimeout { get; set; }

		byte MulticastDelay { get; set; }
	}
}
