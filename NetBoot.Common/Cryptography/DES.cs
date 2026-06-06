/*
 * DES Encryption for NTLM Authentication
 * Used for LM Hash and LM Response generation
 * Based on MS-NLMP Specification
 */

using System;
using System.Security.Cryptography;

namespace Netboot.Common.Cryptography
{
	/// <summary>
	/// DES encryption for NTLM LM Hash and LM Response
	/// </summary>
	public static class DES
	{
		/// <summary>
		/// Expands a 7-byte key to 8 bytes by adding parity bits
		/// </summary>
		public static byte[] ExpandKey(byte[] key56, int position = 0)
		{
			var key8 = new byte[8];

			key8[0] = key56[position + 0];
			key8[1] = (byte)((key56[position + 0] << 7) | (key56[position + 1] >> 1));
			key8[2] = (byte)((key56[position + 1] << 6) | (key56[position + 2] >> 2));
			key8[3] = (byte)((key56[position + 2] << 5) | (key56[position + 3] >> 3));
			key8[4] = (byte)((key56[position + 3] << 4) | (key56[position + 4] >> 4));
			key8[5] = (byte)((key56[position + 4] << 3) | (key56[position + 5] >> 5));
			key8[6] = (byte)((key56[position + 5] << 2) | (key56[position + 6] >> 6));
			key8[7] = (byte)(key56[position + 6] << 1);

			return key8;
		}

		/// <summary>
		/// Encrypts data using DES in ECB mode with no padding
		/// </summary>
		public static byte[] Encrypt(byte[] key, byte[] data)
		{
			var key8 = key.Length == 7 ? ExpandKey(key, 0) : key;
			var result = new byte[0];

			using (var des = global::System.Security.Cryptography.DES.Create())
			{
				des.Mode = CipherMode.ECB;
				des.Padding = PaddingMode.None;
				des.Key = key8;

				using (var ct = des.CreateEncryptor())
					result = ct.TransformFinalBlock(data, 0, data.Length);
			}

			return result;
		}

		/// <summary>
		/// Decrypts data using DES in ECB mode with no padding
		/// </summary>
		public static byte[] Decrypt(byte[] key, byte[] data)
		{
			var key8 = key.Length == 7 ? ExpandKey(key, 0) : key;
			var result = new byte[0];

			using (var des = global::System.Security.Cryptography.DES.Create())
			{
				des.Mode = CipherMode.ECB;
				des.Padding = PaddingMode.None;
				des.Key = key8;

				using (var ct = des.CreateDecryptor())
					result = ct.TransformFinalBlock(data, 0, data.Length);
			}

			return result;
		}
	}
}