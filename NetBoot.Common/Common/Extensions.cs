using System.Buffers.Binary;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Netboot.Common
{
	public static class Extensions
	{
		public static string Captitalize(this string str) => str.First().ToString().ToUpper() + str.Substring(1);

		public static bool ContainsChar(this string str, char[] patterns)
		{
			bool flag = false;
			foreach (char pattern in patterns)
			{
				if (str.Contains(pattern))
					flag = true;
			}

			return flag;
		}

		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			var result = new T[length];
			Array.Copy(data, index, result, 0, length);
		
			return result;
		}

		public static uint ReadUint32LE(this Stream stream)
		{
			var buffer = new byte[sizeof(uint)];
			stream.Read(buffer, 0, buffer.Length);
			
			return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
		}

		public static ushort ReadUint16LE(this Stream stream)
		{
			var buffer = new byte[sizeof(ushort)];
			stream.Read(buffer, 0, buffer.Length);

			return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
		}

		public static string ReadString(this Stream stream, int size, Encoding encoding)
		{
			var buffer = new byte[size];
			stream.Read(buffer, 0, buffer.Length);

			return encoding.GetString(buffer);
		}


		public static double AsUnixTimeStamp(this DateTime dt)
			=> DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

		public static string Append(this string str, string seperator, string appending)
			=> string.Join(seperator,
		[
			str,
			appending
		]);

		public static DateTime FirstDayOfWeek()
		{
			var dayOfWeek = (int)DateTime.Now.DayOfWeek;
			return DateTime.Today.AddDays(dayOfWeek == 0 ? -6.0 : -dayOfWeek + 1);
		}

		public static short GetInt16(this byte[] input)
			=> IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input));

		public static string GetString(this byte[] input, Encoding encoding)
			=> encoding.GetString(input);

		public static string GetString(this string input, Encoding encoding)
			=> encoding.GetString(encoding.GetBytes(input));

		public static byte[] GetBytes_UTF8(this string str) => string.IsNullOrEmpty(str) ? [] : Encoding.UTF8.GetBytes(str);
	}
}
