/*
 * NTLM Cryptography Library - Wrapper
 * Delegates to Netboot.Common.Cryptography.NTLM
 */

using NTLMCore = Netboot.Common.Cryptography.NTLM.NTLM;

namespace NetBoot.NTLM.Cryptography
{
    /// <summary>
    /// NTLM Crypto operations - thin wrapper around Common.NTLM
    /// </summary>
    public static class NTLMCrypto
    {
        /// <summary>
        /// Generates an 8-byte random challenge
        /// </summary>
        public static byte[] GenerateChallenge() => NTLMCore.GenerateChallenge();

        /// <summary>
        /// Generates LM Hash from password (DES-based, uses KGS!@#$% magic)
        /// </summary>
        public static byte[] GenerateLMHash(string password) => NTLMCore.GenerateLMHash(password);

        /// <summary>
        /// Generates NT Hash from password (MD4-based)
        /// </summary>
        public static byte[] GenerateNTHash(string password) => NTLMCore.GenerateNTHash(password);

        /// <summary>
        /// Generates LM Response from hash and challenge (for NTLM v1)
        /// </summary>
        public static byte[] GenerateLMResponse(byte[] lmHash, byte[] challenge) =>
            NTLMCore.GenerateLMResponse(lmHash, challenge);

        /// <summary>
        /// Generates NT Response (HMAC-MD5 based for NTLM v2)
        /// </summary>
        public static byte[] GenerateNTResponse(byte[] ntHash, byte[] serverChallenge, byte[] clientChallenge) =>
            NTLMCore.GenerateNTResponse(ntHash, serverChallenge, clientChallenge);

        /// <summary>
        /// Derives Session Base Key from NTProofStr and ClientChallenge
        /// </summary>
        public static byte[] DeriveSessionBaseKey(byte[] ntProofStr, byte[] clientChallenge) =>
            NTLMCore.DeriveSessionBaseKey(ntProofStr, clientChallenge);

        /// <summary>
        /// Derives Seal Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSealKey(byte[] sessionBaseKey) =>
            NTLMCore.DeriveSealKey(sessionBaseKey, isServer: true);

        /// <summary>
        /// Derives Sign Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSignKey(byte[] sessionBaseKey) =>
            NTLMCore.DeriveSignKey(sessionBaseKey, isServer: true);

        /// <summary>
        /// RC4 Encrypt (seal) data
        /// </summary>
        public static byte[] RC4Seal(byte[] key, byte[] data) => NTLMCore.RC4Seal(key, data);

        /// <summary>
        /// RC4 Decrypt (unseal) data
        /// </summary>
        public static byte[] RC4Unseal(byte[] key, byte[] data) => NTLMCore.RC4Unseal(key, data);

        /// <summary>
        /// Compute NTLM Signature (first 8 bytes of HMAC-MD5)
        /// </summary>
        public static byte[] ComputeSignature(byte[] sealKey, uint sequenceNumber, byte[] data) =>
            NTLMCore.ComputeSignature(sealKey, sequenceNumber, data);

        /// <summary>
        /// Verify NTLM Signature
        /// </summary>
        public static bool VerifySignature(byte[] sealKey, uint sequenceNumber, byte[] data, byte[] expectedSignature) =>
            NTLMCore.VerifySignature(sealKey, sequenceNumber, data, expectedSignature);
    }
}