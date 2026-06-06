using Netboot.Common.Network.Interfaces;
using NetBoot.NTLM.Cryptography;
using System.Net;
using System.Net.Sockets;

namespace Netboot.Module.BINLListener
{
	public class BINLClient
	{
		public string OSCFileName { get; private set; } = "welcome.osc";
		public string Language { get; private set; } = "englisch";
		public bool NTLMV2Enabled { get; private set; } = false;
		public Guid Socket { get; set; }
		public Guid Server { get; set; }
		public Guid Client { get; set; }
		public BINLPacket Response { get; set; }
		public BINLPacket Request { get; set; }

		// NTLM Security Context
		public byte[] ServerChallenge { get; set; }
		public byte[] ClientChallenge { get; set; }
		public byte[] NTProofStr { get; set; }
		public byte[] SessionBaseKey { get; set; }
		public byte[] SealKey { get; set; }
		public byte[] SignKey { get; set; }

		// Security state
		public bool IsAuthenticated { get; set; }
		public bool HasKeys => SessionBaseKey != null && SealKey != null;

		public BINLClient(bool testClient, Guid server, Guid socket, Guid client, BINLPacket request)
		{
			Socket = socket;
			Client = client;
			Server = server;
			Request = request;
		}

		/// <summary>
		/// Creates the NTLM Seal Context from the session keys
		/// </summary>
		public void CreateSealContext()
		{
			if (SessionBaseKey != null)
			{
				SealKey = NTLMCrypto.DeriveSealKey(SessionBaseKey);
				SignKey = NTLMCrypto.DeriveSignKey(SessionBaseKey);
			}
		}

		/// <summary>
		/// Seals (encrypts) data using the session seal key
		/// </summary>
		public byte[] SealData(byte[] data, uint sequenceNumber)
		{
			if (SealKey == null) throw new InvalidOperationException("No seal key available");
			return NTLMCrypto.RC4Seal(SealKey, data);
		}

		/// <summary>
		/// Unseals (decrypts) data using the session seal key
		/// </summary>
		public byte[] UnsealData(byte[] sealedData, uint sequenceNumber)
		{
			if (SealKey == null) throw new InvalidOperationException("No seal key available");
			return NTLMCrypto.RC4Unseal(SealKey, sealedData);
		}

		/// <summary>
		/// Computes the signature for a message
		/// </summary>
		public byte[] ComputeSignature(byte[] data, uint sequenceNumber)
		{
			if (SealKey == null) throw new InvalidOperationException("No seal key available");
			return NTLMCrypto.ComputeSignature(SealKey, sequenceNumber, data);
		}

		/// <summary>
		/// Verifies a message signature
		/// </summary>
		public bool VerifySignature(byte[] data, uint sequenceNumber, byte[] expectedSignature)
		{
			if (SealKey == null) throw new InvalidOperationException("No seal key available");
			return NTLMCrypto.VerifySignature(SealKey, sequenceNumber, data, expectedSignature);
		}
	}
}