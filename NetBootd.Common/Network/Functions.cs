using System.Net;
using System.Net.Sockets;

namespace Netboot.Common
{
	public static partial class Functions
	{
		public static IPHostEntry DNSLookup(string hostOrAddress, AddressFamily addressFamily = AddressFamily.InterNetwork)
			=> Dns.GetHostEntry(hostOrAddress, addressFamily); 

		public static IPHostEntry DNSLookup(IPAddress iPAddress, AddressFamily addressFamily = AddressFamily.InterNetwork)
			=> DNSLookup(iPAddress.ToString(), addressFamily);
	}
}
