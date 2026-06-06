/*
 * NTLM Cryptography - Core Authentication Functions
 * Based on MS-NLMP Specification
 * https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-nlmp
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace Netboot.Common.Cryptography.NTLM
{
    /// <summary>
    /// NTLM Authentication - LM Hash, NT Hash, Responses, Session Keys
    /// </summary>
    public static class NTLM
    {
        // Magic constant for LM Hash
        private static readonly byte[] LM_MAGIC = Encoding.ASCII.GetBytes("KGS!@#$%");

        // Magic constants for session key derivation
        private static readonly byte[] CLIENT_SIGNING_CONSTANT = Encoding.ASCII.GetBytes("session key to client-to-server signing key magic constant");
        private static readonly byte[] SERVER_SIGNING_CONSTANT = Encoding.ASCII.GetBytes(" session key to server-to-client signing key magic constant");
        private static readonly byte[] CLIENT_SEALING_CONSTANT = Encoding.ASCII.GetBytes("session key to client-to-server sealing key magic constant");
        private static readonly byte[] SERVER_SEALING_CONSTANT = Encoding.ASCII.GetBytes("session key to server-to-client sealing key magic constant");

        #region Challenge Generation

        /// <summary>
        /// Generates an 8-byte random challenge
        /// </summary>
        public static byte[] GenerateChallenge()
        {
            byte[] challenge = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(challenge);
            }
            return challenge;
        }

        #endregion

        #region LM Hash (DES-based with KGS!@#$%)

        /// <summary>
        /// Generates LM Hash from password (DES-based, uses KGS!@#$% magic)
        /// </summary>
        public static byte[] GenerateLMHash(string password)
        {
            byte[] lmHash = new byte[16];

            if (string.IsNullOrEmpty(password))
            {
                Buffer.BlockCopy(LM_MAGIC, 0, lmHash, 0, 8);
                Buffer.BlockCopy(LM_MAGIC, 0, lmHash, 8, 8);
                return lmHash;
            }

            var upperPassword = password.ToUpper();
            var key1 = DES.ExpandKey(Encoding.ASCII.GetBytes(upperPassword.Substring(0, Math.Min(7, upperPassword.Length))), 0);
            var key2 = upperPassword.Length >= 8
                ? DES.ExpandKey(Encoding.ASCII.GetBytes(upperPassword.Substring(7, Math.Min(7, upperPassword.Length - 7))), 0)
                : LM_MAGIC;

            using (var des = global::System.Security.Cryptography.DES.Create())
            {
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;

                des.Key = key1;
                using (var ct = des.CreateEncryptor())
                {
                    ct.TransformBlock(LM_MAGIC, 0, 8, lmHash, 0);
                }

                des.Key = key2;
                using (var ct = des.CreateEncryptor())
                {
                    ct.TransformBlock(LM_MAGIC, 0, 8, lmHash, 8);
                }
            }

            return lmHash;
        }

        #endregion

        #region NT Hash (MD4-based)

        /// <summary>
        /// Generates NT Hash from password (MD4-based)
        /// </summary>
        public static byte[] GenerateNTHash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new byte[16];
            }

            var passwordBytes = Encoding.Unicode.GetBytes(password);
            using var md4 = new MD4();
            return md4.ComputeHash(passwordBytes);
        }

        #endregion

        #region Responses

        /// <summary>
        /// Generates LM Response from hash and challenge (for NTLM v1)
        /// </summary>
        public static byte[] GenerateLMResponse(byte[] lmHash, byte[] challenge)
        {
            byte[] response = new byte[24];

            using (var des = global::System.Security.Cryptography.DES.Create())
            {
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;

                var key1 = DES.ExpandKey(lmHash, 0);
                des.Key = key1;
                using (var ct = des.CreateEncryptor())
                {
                    ct.TransformBlock(challenge, 0, 8, response, 0);
                }

                var key2 = DES.ExpandKey(lmHash, 7);
                des.Key = key2;
                using (var ct = des.CreateEncryptor())
                {
                    ct.TransformBlock(challenge, 0, 8, response, 8);
                }
            }

            return response;
        }

        /// <summary>
        /// Generates NT Response (HMAC-MD5 based for NTLM v2)
        /// </summary>
        public static byte[] GenerateNTResponse(byte[] ntHash, byte[] serverChallenge, byte[] clientChallenge)
        {
            var challengeData = new byte[16];
            Buffer.BlockCopy(serverChallenge, 0, challengeData, 0, 8);
            Buffer.BlockCopy(clientChallenge, 0, challengeData, 8, 8);

            using var hmac = new HMACMD5(ntHash);
            return hmac.ComputeHash(challengeData);
        }

        #endregion

        #region Session Key Derivation

        /// <summary>
        /// Derives Session Base Key from NTProofStr and ClientChallenge
        /// </summary>
        public static byte[] DeriveSessionBaseKey(byte[] ntProofStr, byte[] clientChallenge)
        {
            using var hmac = new HMACMD5(ntProofStr);
            return hmac.ComputeHash(clientChallenge);
        }

        /// <summary>
        /// Derives Seal Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSealKey(byte[] sessionBaseKey, bool isServer = true)
        {
            var constant = isServer ? SERVER_SEALING_CONSTANT : CLIENT_SEALING_CONSTANT;
            using var hmac = new HMACMD5(sessionBaseKey);
            return hmac.ComputeHash(constant);
        }

        /// <summary>
        /// Derives Sign Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSignKey(byte[] sessionBaseKey, bool isServer = true)
        {
            var constant = isServer ? SERVER_SIGNING_CONSTANT : CLIENT_SIGNING_CONSTANT;
            using var hmac = new HMACMD5(sessionBaseKey);
            return hmac.ComputeHash(constant);
        }

        #endregion

        #region RC4 Seal/Unseal

        /// <summary>
        /// RC4 Encrypt (seal) data
        /// </summary>
        public static byte[] RC4Seal(byte[] key, byte[] data)
        {
            return RC4.Crypt(key, data);
        }

        /// <summary>
        /// RC4 Decrypt (unseal) data
        /// </summary>
        public static byte[] RC4Unseal(byte[] key, byte[] data)
        {
            return RC4.Crypt(key, data);
        }

        #endregion

        #region Signature

        /// <summary>
        /// Compute NTLM Signature (first 8 bytes of HMAC-MD5)
        /// </summary>
        public static byte[] ComputeSignature(byte[] sealKey, uint sequenceNumber, byte[] data)
        {
            var toSign = new byte[4 + data.Length];
            BitConverter.GetBytes(sequenceNumber).CopyTo(toSign, 0);
            data.CopyTo(toSign, 4);
            using var hmac = new HMACMD5(sealKey);
            var hmacResult = hmac.ComputeHash(toSign);

            var sig = new byte[8];
            Buffer.BlockCopy(hmacResult, 0, sig, 0, 8);
            return sig;
        }

        /// <summary>
        /// Verify NTLM Signature
        /// </summary>
        public static bool VerifySignature(byte[] sealKey, uint sequenceNumber, byte[] data, byte[] expectedSignature)
        {
            var computed = ComputeSignature(sealKey, sequenceNumber, data);
            return ArrayEquals(computed, expectedSignature);
        }

        #endregion

        #region Private Helpers

        private static bool ArrayEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        #endregion
    }
}