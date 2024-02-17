using System.Net;
using System.Net.NetworkInformation;

namespace Netboot.Common
{
	public static partial class Functions
    {
		public static string ReplaceSlashes(string input)
		{
			var slash = "/";

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				case PlatformID.Xbox:
					slash = "\\";
					break;
				default:
				case PlatformID.Other:
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					slash = "/";
					break;

			}

			return input.Replace("/", slash);
		}

		public static void InvokeMethod(object obj, string name, object?[]? args)
        {
            try
            {
                var methods = obj.GetType().GetMethods().Where(m => m.Name == name && m.IsPublic).FirstOrDefault();
                methods.Invoke(obj, args);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static IEnumerable<IPAddress> GetIPAddresses()
        {
			return from ni in NetworkInterface.GetAllNetworkInterfaces()
				   from ip in ni.GetIPProperties().UnicastAddresses
				   where !IPAddress.IsLoopback(ip.Address) && ip.Address.GetAddressBytes()[0] != 0xa9
				   select ip.Address;
		}

        public static bool IsLittleEndian() => BitConverter.IsLittleEndian;
    }
}
