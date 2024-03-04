/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
			#region "Generate the Seed"
			var SysTime = BitConverter.GetBytes(NTQuerySystemTime());
			var Seed = ((SysTime[1] + 1) << 0) | ((SysTime[2] + 0) << 8)
				| ((SysTime[3] - 1) << 16) | ((SysTime[4] + 0) << 24);
			Seed *= 0x100;
			#endregion

			#region "Generate the Challenge"
			var rand = new Random(Seed);

			var ulChallenge = new uint[2];
			var ulNegate = (uint)rand.Next();

			for (var i = 0; i < ulChallenge.Length; i++)
				ulChallenge[i] = (uint)rand.Next();

			var x = ulNegate & 0x1;
			var y = ulNegate & 0x2;

			if (x == 1)
				ulChallenge[0] |= 0x80000000;
			if (y == 1)
				ulChallenge[1] |= 0x80000000;
			#endregion

			#region "Create the challenge Buffer"
			var challenge = new byte[2 * sizeof(uint)];

			var chal0 = BitConverter.GetBytes(ulChallenge[0]);
			var chal1 = BitConverter.GetBytes(ulChallenge[1]);

			Array.Copy(chal0, 0, challenge, 0, chal0.Length);
			Array.Copy(chal1, 0, challenge, sizeof(uint), chal1.Length);
			#endregion

			return challenge;
		}

		public static string ReplaceSlashes(string input)
		{
			return input.Replace("/", NetbootBase.Platform.DirectorySeperatorChar);
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
			return from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
				   from unicastAddress in networkInterface.GetIPProperties().UnicastAddresses
				   where !IPAddress.IsLoopback(unicastAddress.Address) &&
				   unicastAddress.Address.GetAddressBytes()[0] != 0xa9
				   select unicastAddress.Address;
		}

		public static bool IsLittleEndian() => BitConverter.IsLittleEndian;
	}
}
