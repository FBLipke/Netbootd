using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netboot.Module.BINLListener;
using NetBoot.NTLM.Cryptography;

namespace NetBoot.Tests
{
	/// <summary>
	/// BINL Protocol Tests
	/// Tests the BINL (Boot Information Negotiation Layer) protocol implementation
	/// including message types, packet structures, and NTLM crypto operations.
	/// </summary>
	[TestClass]
	public class BINLTests
	{
		// Path to test data files (binary test fixtures for protocol testing)
		private static string _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

		#region "Infrastructure Tests"

		/// <summary>
		/// PREREQUISITE TEST: Verifies TestData directory exists before any other tests run.
		/// This ensures all binary test fixtures are available for subsequent tests.
		/// If this fails, no other tests can meaningfully execute.
		/// </summary>
		[TestMethod]
		public void TestData_Directory_Exists()
		{
			Assert.IsTrue(Directory.Exists(_testDataPath), $"TestData directory not found at: {_testDataPath}");
		}

		#endregion

		#region "BINL Protocol Constants Tests"

		/// <summary>
		/// Tests that BINL message type constants match the expected values from specification.
		/// These magic constants define the BINL protocol message types:
		/// - Authenticate (0x81415554): Client authentication response
		/// - Challenge (0x8243484C): Server challenge to client
		/// - Negotiate (0x814E4547): Initial negotiation request
		/// </summary>
		[TestMethod]
		public void BINLMessageTypes_AreCorrect()
		{
			Assert.AreEqual((uint)0x81415554, (uint)BINLMessageTypes.Authenticate, "BINL Authenticate message type mismatch");
			Assert.AreEqual((uint)0x8243484c, (uint)BINLMessageTypes.Challenge, "BINL Challenge message type mismatch");
			Assert.AreEqual((uint)0x814e4547, (uint)BINLMessageTypes.Negotiate, "BINL Negotiate message type mismatch");
		}

		#endregion

		#region "BINL Packet Structure Tests"

		/// <summary>
		/// Tests the RQU (Request) packet structure by verifying the sequence number field.
		/// RIS clients send multiple fragments of boot information, identified by sequence numbers.
		/// </summary>
		[TestMethod]
		public void ris_rqu_HasSequence()
		{
			var path = Path.Combine(_testDataPath, "ris_rqu.bin");
			if (!File.Exists(path)) Assert.Inconclusive("ris_rqu.bin not found - cannot test packet structure");

			var data = File.ReadAllBytes(path);
			// Sequence number is at offset 8, 2 bytes, little-endian
			var sequence = BitConverter.ToUInt16(data, 8);
			Assert.AreEqual((ushort)2, sequence, "RQU packet sequence number should be 2");
		}

		#endregion

		#region "NTLM Cryptography Tests"

		/// <summary>
		/// Tests the cryptographic challenge generation.
		/// NTLM authentication requires an 8-byte challenge from the server.
		/// This verifies the crypto library can generate proper random challenges.
		/// </summary>
		[TestMethod]
		public void NTLMCrypto_GenerateChallenge_Returns8Bytes()
		{
			var challenge = NTLMCrypto.GenerateChallenge();
			Assert.AreEqual(8, challenge.Length, "NTLM challenge must be exactly 8 bytes");
		}

		/// <summary>
		/// Tests the RC4 seal/unseal roundtrip.
		/// NTLM v2 uses RC4 for encrypting channel traffic after authentication.
		/// This verifies the symmetric encryption/decryption works correctly.
		/// </summary>
		[TestMethod]
		public void NTLMCrypto_RC4SealUnseal_Roundtrip()
		{
			var key = new byte[16]; // 128-bit RC4 key
			var plaintext = new byte[] { 0x11, 0x22, 0x33, 0x44 };

			// Seal (encrypt) the data
			var sealedData = NTLMCrypto.RC4Seal(key, plaintext);

			// Unseal (decrypt) the data - should match original plaintext
			var unsealedData = NTLMCrypto.RC4Unseal(key, sealedData);

			CollectionAssert.AreEqual(plaintext, unsealedData, "RC4 unseal must return original plaintext");
		}

		/// <summary>
		/// Tests signature computation for NTLM message integrity.
		/// Each NTLM message includes an 8-byte signature for integrity verification.
		/// </summary>
		[TestMethod]
		public void NTLMCrypto_ComputeSignature_Returns8Bytes()
		{
			var sealKey = new byte[16];
			var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };

			// Compute signature with message number 1
			var sig = NTLMCrypto.ComputeSignature(sealKey, 1, data);

			Assert.AreEqual(8, sig.Length, "NTLM signature must be exactly 8 bytes");
		}

		/// <summary>
		/// Tests signature verification.
		/// Verifies that a valid signature passes verification.
		/// This is critical for ensuring message integrity in the BINL protocol.
		/// </summary>
		[TestMethod]
		public void NTLMCrypto_VerifySignature_Valid_ReturnsTrue()
		{
			var sealKey = new byte[16];
			var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };

			// Generate signature for the data
			var sig = NTLMCrypto.ComputeSignature(sealKey, 1, data);

			// Verify should return true for valid signature
			var valid = NTLMCrypto.VerifySignature(sealKey, 1, data, sig);

			Assert.IsTrue(valid, "Valid signature should pass verification");
		}

		#endregion
	}
}