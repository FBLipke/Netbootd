using System.Text;

namespace Netboot.Common.Netboot.Common
{
	public static class Extensions
	{
		public static string GetString(this byte[] input)
			=> GetString(input, Encoding.ASCII);

		public static string GetString(this byte[] input, Encoding encoding)
			=> encoding.GetString(input);
	}
}
