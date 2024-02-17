using System.Buffers.Binary;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Netboot.Common
{
	public static class Extensions
	{
		public static string GetString(this byte[] input)
			=> GetString(input, Encoding.ASCII);

		public static short Get_Int16(this byte[] input)
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input));
		}

		public static string GetString(this byte[] input, Encoding encoding)
			=> encoding.GetString(input);
	}
}
