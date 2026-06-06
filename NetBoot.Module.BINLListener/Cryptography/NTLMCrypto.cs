/*
 * NTLM Sealing/Unsealing Implementation
 * Based on MS-NLMP specification
 */

using System;
using System.Security.Cryptography;
using Netboot.Common.Cryptography;

namespace Netboot.Module.BINLListener.Cryptography
{
	/// <summary>
	/// RC4 encryption engine for NTLM sealing/unsealing
	/// </summary>
	public class RC4Engine
	{
		/// <summary>
		/// Encrypts or decrypts data using RC4 (self-inverting)
		/// </summary>
		public static byte[] Crypt(byte[] key, byte[] data)
		{
			return RC4.Crypt(key, data);
		}
	}

	/// <summary>
	/// NTLM Sealing context - manages session keys and sealing operations
	/// </summary>
	public class NTLMSealContext
	{
		private byte[] _sessionKey;
		private byte[] _sealKey;
		private byte[] _signKey;

		public static NTLMSealContext CreateFromSessionKey(byte[] sessionKey)
		{
			var context = new NTLMSealContext();
			context._sessionKey = sessionKey;

			// Derive seal key: HMAC_MD5(sessionKey, "session key to server-to-client sealing key magic constant")
			context._sealKey = ComputeHMAC_MD5(sessionKey, "session key to server-to-client sealing key magic constant"u8.ToArray());

			// Derive sign key: HMAC_MD5(sessionKey, "session key to server-to-client sealing key magic constant")
			context._signKey = ComputeHMAC_MD5(sessionKey, "session key to server-to-client sealing key magic constant"u8.ToArray());

			return context;
		}

		/// <summary>
		/// Unseals (decrypts) NTLM message data
		/// </summary>
		public byte[] Unseal(byte[] sealedData, uint sequenceNumber)
		{
			return RC4Engine.Crypt(_sealKey, sealedData);
		}

		/// <summary>
		/// Seals (encrypts) NTLM message data
		/// </summary>
		public byte[] Seal(byte[] plainData, uint sequenceNumber)
		{
			return RC4Engine.Crypt(_sealKey, plainData);
		}

		/// <summary>
		/// Computes 8-byte signature for verification
		/// </summary>
		public byte[] ComputeSignature(byte[] data, uint sequenceNumber)
		{
			// Sig = HMAC_MD5(sealKey, seqNum + data)
			var toSign = new byte[4 + data.Length];
			BitConverter.GetBytes(sequenceNumber).CopyTo(toSign, 0);
			data.CopyTo(toSign, 4);
			return ComputeHMAC_MD5(_sealKey, toSign);
		}

		/// <summary>
		/// Verifies 8-byte signature
		/// </summary>
		public bool VerifySignature(byte[] data, uint sequenceNumber, byte[] expectedSignature)
		{
			var computed = ComputeSignature(data, sequenceNumber);
			return computed.AsSpan().SequenceEqual(expectedSignature.AsSpan());
		}

		public byte[] SessionKey => _sessionKey;
		public byte[] SealKey => _sealKey;
		public byte[] SignKey => _signKey;

		private static byte[] ComputeHMAC_MD5(byte[] key, byte[] data)
		{
			using var hmac = new Netboot.Common.Cryptography.HMACMD5(key);
			return hmac.ComputeHash(data);
		}
	}
}