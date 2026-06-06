using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetBoot.NTLM.Cryptography;

namespace NetBoot.Tests;

[TestClass]
public class NTLMCryptoTests
{
	[TestMethod]
	public void GenerateChallenge_Returns8Bytes()
	{
		var challenge = NTLMCrypto.GenerateChallenge();
		Assert.AreEqual(8, challenge.Length);
	}

	[TestMethod]
	public void GenerateLMHash_Administrator_Returns16Bytes()
	{
		var lmHash = NTLMCrypto.GenerateLMHash("Administrator");
		Assert.AreEqual(16, lmHash.Length);
	}

	[TestMethod]
	public void GenerateNTHash_Administrator_Returns16Bytes()
	{
		var ntHash = NTLMCrypto.GenerateNTHash("Administrator");
		Assert.AreEqual(16, ntHash.Length);
	}

	[TestMethod]
	public void GenerateLMResponse_Returns24Bytes()
	{
		var lmHash = NTLMCrypto.GenerateLMHash("TestPassword");
		var challenge = new byte[8];
		var lmResp = NTLMCrypto.GenerateLMResponse(lmHash, challenge);
		Assert.AreEqual(24, lmResp.Length);
	}

	[TestMethod]
	public void GenerateNTResponse_Returns16Bytes()
	{
		var ntHash = new byte[16];
		var serverChallenge = new byte[8];
		var clientChallenge = new byte[8];
		var ntResp = NTLMCrypto.GenerateNTResponse(ntHash, serverChallenge, clientChallenge);
		Assert.AreEqual(16, ntResp.Length);
	}

	[TestMethod]
	public void DeriveSealKey_Returns16Bytes()
	{
		var sessionKey = new byte[16];
		var sealKey = NTLMCrypto.DeriveSealKey(sessionKey);
		Assert.AreEqual(16, sealKey.Length);
	}

	[TestMethod]
	public void DeriveSignKey_Returns16Bytes()
	{
		var sessionKey = new byte[16];
		var signKey = NTLMCrypto.DeriveSignKey(sessionKey);
		Assert.AreEqual(16, signKey.Length);
	}

	[TestMethod]
	public void RC4Seal_Unseal_Roundtrip()
	{
		var key = new byte[16];
		var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
		var sealedData = NTLMCrypto.RC4Seal(key, data);
		var unsealedData = NTLMCrypto.RC4Unseal(key, sealedData);
		CollectionAssert.AreEqual(data, unsealedData);
	}

	[TestMethod]
	public void ComputeSignature_Returns8Bytes()
	{
		var sealKey = new byte[16];
		var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
		var sig = NTLMCrypto.ComputeSignature(sealKey, 1, data);
		Assert.AreEqual(8, sig.Length);
	}

	[TestMethod]
	public void VerifySignature_ValidSignature_ReturnsTrue()
	{
		var sealKey = new byte[16];
		var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
		var sig = NTLMCrypto.ComputeSignature(sealKey, 1, data);
		var valid = NTLMCrypto.VerifySignature(sealKey, 1, data, sig);
		Assert.IsTrue(valid);
	}

	[TestMethod]
	public void VerifySignature_InvalidSignature_ReturnsFalse()
	{
		var sealKey = new byte[16];
		var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
		var fakeSig = new byte[8];
		var valid = NTLMCrypto.VerifySignature(sealKey, 1, data, fakeSig);
		Assert.IsFalse(valid);
	}
}