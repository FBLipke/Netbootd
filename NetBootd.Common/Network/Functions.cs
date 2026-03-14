using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Netboot.Common
{
	public static partial class Functions
	{
		public static IPHostEntry DNSLookup(string hostOrAddress, AddressFamily addressFamily = AddressFamily.InterNetwork)
			=> Dns.GetHostEntry(hostOrAddress, addressFamily); 

		public static IPHostEntry DNSLookup(IPAddress iPAddress, AddressFamily addressFamily = AddressFamily.InterNetwork)
			=> DNSLookup(iPAddress.ToString(), addressFamily);

		public static PhysicalAddress GetMacAddress()
		{
            var mac = NetworkInterface.GetAllNetworkInterfaces()
        .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
        .Select(n => n.GetPhysicalAddress())
        .FirstOrDefault();


            return mac;
		}
	}
}
