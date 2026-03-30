using Netboot.Module.DHCPListener;
using System.Net;


namespace DHCPListener.BSvcMod.MSRIS
{
	public interface IRISClient : IDHCPClient, IDisposable
	{
		public Guid Id { get; set; }
	}
}
