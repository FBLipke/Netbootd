using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Netboot.Common
{
	public static partial class Functions
    {

		public static long NTQuerySystemTime()
		{
			var nano = 10000L * Stopwatch.GetTimestamp();
			nano /= TimeSpan.TicksPerMillisecond;
			nano *= 100L;
		
			return nano;
		}

		public static byte[] NTLMChallenge()
		{
			var SysTime = BitConverter.GetBytes(NTQuerySystemTime());
			var Seed = ((SysTime[1] + 1) << 0) | ((SysTime[2] + 0) << 8)
				| ((SysTime[3] - 1) << 16) | ((SysTime[4] + 0) << 24);
			Seed *= 0x100;

			var rand = new Random(Seed);

			var ulChallenge = new uint[2];
			var ulNegate = (uint)rand.Next();

			for (int i = 0; i < ulChallenge.Length; i++)
				ulChallenge[i] = (uint)rand.Next();

			var x = ulNegate & 0x1;
			var y = ulNegate & 0x2;

			if (x == 1)
				ulChallenge[0] |= 0x80000000;
			if (y == 1)
				ulChallenge[1] |= 0x80000000;

			var challenge = new byte[2 * sizeof(uint)];

			var chal0 = BitConverter.GetBytes(ulChallenge[0]);
			var chal1 = BitConverter.GetBytes(ulChallenge[1]);

			Array.Copy(chal0, 0, challenge, 0, chal0.Length);
			Array.Copy(chal1, 0, challenge, sizeof(uint), chal1.Length);
			return challenge;

		}

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
