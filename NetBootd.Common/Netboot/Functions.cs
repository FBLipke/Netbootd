using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetBoot
{
	public static class Functions
	{
		public static IList<IPAddress> GetIPAddresses()
		{
			var addresses = new List<IPAddress>();

			foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
				if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
					foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
						if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
							if (!IPAddress.IsLoopback(ip.Address) && ip.Address.GetAddressBytes()[0] != 0xa9)
								addresses.Add(ip.Address);

			return addresses;
		}

		public static bool IsLittleEndian() => BitConverter.IsLittleEndian;
	}
}
