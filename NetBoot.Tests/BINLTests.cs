using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netboot.Module.BINLListener;
using NetBoot.NTLM.Cryptography;

namespace NetBoot.Tests
{
    [TestClass]
    public class BINLTests
    {
        private static string _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

        [TestMethod]
        public void BINLMessageTypes_AreCorrect()
        {
            Assert.AreEqual((uint)0x81415554, (uint)BINLMessageTypes.Authenticate);
            Assert.AreEqual((uint)0x8243484c, (uint)BINLMessageTypes.Challenge);
            Assert.AreEqual((uint)0x814e4547, (uint)BINLMessageTypes.Negotiate);
        }

        [TestMethod]
        public void ris_rqu_HasSequence()
        {
            var path = Path.Combine(_testDataPath, "ris_rqu.bin");
            if (!File.Exists(path)) Assert.Inconclusive();
            var data = File.ReadAllBytes(path);
            var sequence = BitConverter.ToUInt16(data, 8);
            Assert.AreEqual((ushort)2, sequence);
        }

        [TestMethod]
        public void NTLMCrypto_GenerateChallenge_Returns8Bytes()
        {
            var challenge = NTLMCrypto.GenerateChallenge();
            Assert.AreEqual(8, challenge.Length);
        }

        [TestMethod]
        public void NTLMCrypto_RC4SealUnseal_Roundtrip()
        {
            var key = new byte[16];
            var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            var sealedData = NTLMCrypto.RC4Seal(key, data);
            var unsealedData = NTLMCrypto.RC4Unseal(key, sealedData);
            CollectionAssert.AreEqual(data, unsealedData);
        }

        [TestMethod]
        public void NTLMCrypto_ComputeSignature_Returns8Bytes()
        {
            var sealKey = new byte[16];
            var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            var sig = NTLMCrypto.ComputeSignature(sealKey, 1, data);
            Assert.AreEqual(8, sig.Length);
        }

        [TestMethod]
        public void NTLMCrypto_VerifySignature_Valid_ReturnsTrue()
        {
            var sealKey = new byte[16];
            var data = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            var sig = NTLMCrypto.ComputeSignature(sealKey, 1, data);
            var valid = NTLMCrypto.VerifySignature(sealKey, 1, data, sig);
            Assert.IsTrue(valid);
        }
    }
}
