/*
 * HMAC-MD5 Implementation
 * Based on RFC 2104
 */

using System;

namespace Netboot.Common.Cryptography
{
	/// <summary>
	/// HMAC-MD5 keyed-hash message authentication code
	/// </summary>
	public class HMACMD5 : IDisposable
	{
		private readonly MD4 _md4 = new MD4();
		private byte[] _innerKey;
		private byte[] _outerKey;
		private bool _disposed;

		public HMACMD5(byte[] key)
		{
			var keyBytes = key;

			// If key is longer than 64 bytes, hash it with MD4
			if (keyBytes.Length > 64)
			{
				keyBytes = _md4.ComputeHash(keyBytes);
			}

			// Pad key to 64 bytes
			var paddedKey = new byte[64];
			Buffer.BlockCopy(keyBytes, 0, paddedKey, 0, Math.Min(keyBytes.Length, 64));

			// Create inner and outer keys (XOR with constants)
			_innerKey = new byte[64];
			_outerKey = new byte[64];

			for (int i = 0; i < 64; i++)
			{
				_innerKey[i] = (byte)(paddedKey[i] ^ 0x36);
				_outerKey[i] = (byte)(paddedKey[i] ^ 0x5c);
			}
		}

		/// <summary>
		/// Computes HMAC-MD5 hash
		/// </summary>
		public byte[] ComputeHash(byte[] data)
		{
			// Inner hash: MD4(innerKey || data)
			var innerData = new byte[_innerKey.Length + data.Length];
			Buffer.BlockCopy(_innerKey, 0, innerData, 0, _innerKey.Length);
			Buffer.BlockCopy(data, 0, innerData, _innerKey.Length, data.Length);
			var innerHash = _md4.ComputeHash(innerData);

			// Outer hash: MD4(outerKey || innerHash)
			var outerData = new byte[_outerKey.Length + innerHash.Length];
			Buffer.BlockCopy(_outerKey, 0, outerData, 0, _outerKey.Length);
			Buffer.BlockCopy(innerHash, 0, outerData, _outerKey.Length, innerHash.Length);
			return _md4.ComputeHash(outerData);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
			}
		}
	}
}