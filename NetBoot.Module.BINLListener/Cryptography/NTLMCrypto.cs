/*
 * NTLM Sealing/Unsealing Implementation
 * Based on MS-NLMP specification
 */

using Netboot.Common.Cryptography;

namespace Netboot.Module.BINLListener
{

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
            using var hmac = new HMACMD5(key);
            return hmac.ComputeHash(data);
        }
    }

    public enum NTLMMessageType : uint
    {
        Negotiation = 1,
        Challenge = 2,
        Authenticate = 3
    }

    [Flags]
    public enum NTLMNegotiateFlags : uint
    {
        Unicode = 0x00000001,
        OEM = 0x00000002,
        RequestTarget = 0x00000004,
        Sign = 0x00000010,
        Seal = 0x00000020,
        Challenge = 0x00000080,
        AlwaysSign = 0x00004000,
        NTLM2Key = 0x00080000,
        State32 = 0x80000000
    }

    public enum BINLPacketTag : uint
    {
        NEG = 0x814e4547,
        CHL = 0x8243484c,
        AUT = 0x81415554,
        AU2 = 0x81415532,
        NCQ = 0x814e4351,
        NCR = 0x824e4352,
        HLQ = 0x81484c51,
        HLR = 0x82484c52,
        RQU = 0x81525155,
        RSU = 0x82525355,
        REQ = 0x81525145,
        RES = 0x82524553,
        RSP = 0x82525053,
        OFF = 0x82464f46
    }
}