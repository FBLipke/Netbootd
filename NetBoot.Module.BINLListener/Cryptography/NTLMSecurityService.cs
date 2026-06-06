/*
 * NTLM Security Service - Wrapper
 * Delegates to Netboot.Common.Cryptography.NTLM
 */

using Netboot.Common.Cryptography.NTLM;

namespace Netboot.Module.BINLListener.Cryptography
{
    /// <summary>
    /// NTLM Security Service - thin wrapper around Common.NTLM
    /// </summary>
    public static class NTLMSecurityService
    {
        /// <summary>
        /// Generates an 8-byte random challenge
        /// </summary>
        public static byte[] GenerateChallenge() => NTLM.GenerateChallenge();

        /// <summary>
        /// Generates LM Hash from password (DES-based, uses KGS!@#$% magic)
        /// </summary>
        public static byte[] GenerateLMHash(string password) => NTLM.GenerateLMHash(password);

        /// <summary>
        /// Generates NT Hash from password (MD4-based)
        /// </summary>
        public static byte[] GenerateNTHash(string password) => NTLM.GenerateNTHash(password);

        /// <summary>
        /// Generates LM Response from hash and challenge (for NTLM v1)
        /// </summary>
        public static byte[] GenerateLMResponse(byte[] lmHash, byte[] challenge) =>
            NTLM.GenerateLMResponse(lmHash, challenge);

        /// <summary>
        /// Generates NT Response (HMAC-MD5 based for NTLM v2)
        /// </summary>
        public static byte[] GenerateNTResponse(byte[] ntHash, byte[] serverChallenge, byte[] clientChallenge) =>
            NTLM.GenerateNTResponse(ntHash, serverChallenge, clientChallenge);

        /// <summary>
        /// Derives Session Base Key from NTProofStr and ClientChallenge
        /// </summary>
        public static byte[] DeriveSessionBaseKey(byte[] ntProofStr, byte[] clientChallenge) =>
            NTLM.DeriveSessionBaseKey(ntProofStr, clientChallenge);

        /// <summary>
        /// Derives Seal Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSealKey(byte[] sessionBaseKey, bool isServer = true) =>
            NTLM.DeriveSealKey(sessionBaseKey, isServer);

        /// <summary>
        /// Derives Sign Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSignKey(byte[] sessionBaseKey, bool isServer = true) =>
            NTLM.DeriveSignKey(sessionBaseKey, isServer);

        /// <summary>
        /// RC4 Encrypt (seal) data
        /// </summary>
        public static byte[] RC4Seal(byte[] key, byte[] data) => NTLM.RC4Seal(key, data);

        /// <summary>
        /// RC4 Decrypt (unseal) data
        /// </summary>
        public static byte[] RC4Unseal(byte[] key, byte[] data) => NTLM.RC4Unseal(key, data);

        /// <summary>
        /// Compute NTLM Signature
        /// </summary>
        public static byte[] ComputeSignature(byte[] sealKey, uint sequenceNumber, byte[] data) =>
            NTLM.ComputeSignature(sealKey, sequenceNumber, data);

        /// <summary>
        /// Verify NTLM Signature
        /// </summary>
        public static bool VerifySignature(byte[] sealKey, uint sequenceNumber, byte[] data, byte[] expectedSignature) =>
            NTLM.VerifySignature(sealKey, sequenceNumber, data, expectedSignature);
    }
}