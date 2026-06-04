/*
 * NTLM Security Service - Complete NTLM Handshake Handler
 * Combines LM Hash (KGS!@#$%) + NTLM2 Sealing + Session Key Management
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace Netboot.Module.BINLListener.Cryptography
{
    /// <summary>
    /// NTLM Security Service - handles complete NTLM authentication handshake
    /// </summary>
    public class NTLMSecurityService
    {
        // Magic constants from MS-NLMP
        private static readonly byte[] LM_MAGIC = Encoding.ASCII.GetBytes("KGS!@#$%");
        private static readonly byte[] CLIENT_SIGNING_CONSTANT = Encoding.ASCII.GetBytes("session key to client-to-server signing key magic constant");
        private static readonly byte[] SERVER_SIGNING_CONSTANT = Encoding.ASCII.GetBytes(" session key to server-to-client signing key magic constant");
        private static readonly byte[] CLIENT_SEALING_CONSTANT = Encoding.ASCII.GetBytes("session key to client-to-server sealing key magic constant");
        private static readonly byte[] SERVER_SEALING_CONSTANT = Encoding.ASCII.GetBytes("session key to server-to-client sealing key magic constant");

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

        /// <summary>
        /// Generates LM Hash from password (DES-based, uses KGS!@#$% magic)
        /// </summary>
        public static byte[] GenerateLMHash(string password)
        {
            byte[] lmHash = new byte[16];
            
            using (var des = DES.Create())
            {
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;

                if (string.IsNullOrEmpty(password))
                {
                    // Empty password = use magic as LM hash
                    Buffer.BlockCopy(LM_MAGIC, 0, lmHash, 0, 8);
                    Buffer.BlockCopy(LM_MAGIC, 0, lmHash, 8, 8);
                }
                else
                {
                    var upperPassword = password.ToUpper();
                    
                    // First 7 bytes of uppercase password
                    byte[] key1 = PrepareDESKey(upperPassword, 0);
                    using (var ct = des.CreateEncryptor())
                    {
                        ct.TransformBlock(LM_MAGIC, 0, 8, lmHash, 0);
                    }
                    
                    // Second 7 bytes (or magic if password < 8 chars)
                    if (upperPassword.Length >= 8)
                    {
                        byte[] key2 = PrepareDESKey(upperPassword, 7);
                        using (var ct = des.CreateEncryptor())
                        {
                            ct.TransformBlock(LM_MAGIC, 0, 8, lmHash, 8);
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(LM_MAGIC, 0, lmHash, 8, 8);
                    }
                }
            }
            
            return lmHash;
        }

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
            return MD4Hash(passwordBytes);
        }

        /// <summary>
        /// Generates LM Response from hash and challenge (for NTLM v1)
        /// </summary>
        public static byte[] GenerateLMResponse(byte[] lmHash, byte[] challenge)
        {
            byte[] response = new byte[24];
            
            using (var des = DES.Create())
            {
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;

                // First 7 bytes of hash
                byte[] key1 = PrepareDESKey(lmHash, 0);
                des.Key = key1;
                using (var ct = des.CreateEncryptor())
                {
                    ct.TransformBlock(challenge, 0, 8, response, 0);
                }

                // Second 7 bytes of hash
                byte[] key2 = PrepareDESKey(lmHash, 7);
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
            // NTResponse = HMAC-MD5(NTHash, ServerChallenge || ClientChallenge)
            var challengeData = new byte[16];
            Buffer.BlockCopy(serverChallenge, 0, challengeData, 0, 8);
            Buffer.BlockCopy(clientChallenge, 0, challengeData, 8, 8);
            
            return HMACMD5(ntHash, challengeData);
        }

        /// <summary>
        /// Derives Session Base Key from NTProofStr and ClientChallenge
        /// </summary>
        public static byte[] DeriveSessionBaseKey(byte[] ntProofStr, byte[] clientChallenge)
        {
            return HMACMD5(ntProofStr, clientChallenge);
        }

        /// <summary>
        /// Derives Seal Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSealKey(byte[] sessionBaseKey, bool isServer = true)
        {
            var constant = isServer ? SERVER_SEALING_CONSTANT : CLIENT_SEALING_CONSTANT;
            return HMACMD5(sessionBaseKey, constant);
        }

        /// <summary>
        /// Derives Sign Key from Session Base Key
        /// </summary>
        public static byte[] DeriveSignKey(byte[] sessionBaseKey, bool isServer = true)
        {
            var constant = isServer ? SERVER_SIGNING_CONSTANT : CLIENT_SIGNING_CONSTANT;
            return HMACMD5(sessionBaseKey, constant);
        }

        /// <summary>
        /// RC4 Encrypt (seal) data
        /// </summary>
        public static byte[] RC4Seal(byte[] key, byte[] data)
        {
            return RC4Crypt(key, data);
        }

        /// <summary>
        /// RC4 Decrypt (unseal) data
        /// </summary>
        public static byte[] RC4Unseal(byte[] key, byte[] data)
        {
            return RC4Crypt(key, data);
        }

        /// <summary>
        /// Compute NTLM Signature
        /// </summary>
        public static byte[] ComputeSignature(byte[] sealKey, uint sequenceNumber, byte[] data)
        {
            var toSign = new byte[4 + data.Length];
            BitConverter.GetBytes(sequenceNumber).CopyTo(toSign, 0);
            data.CopyTo(toSign, 4);
            return HMACMD5(sealKey, toSign);
        }

        /// <summary>
        /// Verify NTLM Signature
        /// </summary>
        public static bool VerifySignature(byte[] sealKey, uint sequenceNumber, byte[] data, byte[] expectedSignature)
        {
            var computed = ComputeSignature(sealKey, sequenceNumber, data);
            return ArrayEquals(computed, expectedSignature);
        }

        // ===== Private Helper Methods =====

        private static byte[] PrepareDESKey(string password, int position)
        {
            var key7 = new byte[7];
            var passwordUpper = password.ToUpper();
            
            for (int i = 0; i < 7 && (position + i) < passwordUpper.Length; i++)
            {
                key7[i] = (byte)passwordUpper[position + i];
            }

            return ExpandDESKey(key7);
        }

        private static byte[] PrepareDESKey(byte[] key56, int position)
        {
            var key7 = new byte[7];
            
            for (int i = 0; i < 7 && (position + i) < key56.Length; i++)
            {
                key7[i] = key56[position + i];
            }

            return ExpandDESKey(key7);
        }

        private static byte[] ExpandDESKey(byte[] key7)
        {
            var key8 = new byte[8];
            
            key8[0] = key7[0];
            key8[1] = (byte)((key7[0] << 7) | (key7[1] >> 1));
            key8[2] = (byte)((key7[1] << 6) | (key7[2] >> 2));
            key8[3] = (byte)((key7[2] << 5) | (key7[3] >> 3));
            key8[4] = (byte)((key7[3] << 4) | (key7[4] >> 4));
            key8[5] = (byte)((key7[4] << 3) | (key7[5] >> 5));
            key8[6] = (byte)((key7[5] << 2) | (key7[6] >> 6));
            key8[7] = (byte)(key7[6] << 1);

            return key8;
        }

        private static byte[] MD4Hash(byte[] data)
        {
            // MD4 implementation
            var state = new uint[] { 0x67452301, 0xefcdab89, 0x98badcfe, 0x10325476 };
            
            // Padding
            var msgLen = data.Length;
            var padded = new byte[(msgLen + 64) & ~63];
            Buffer.BlockCopy(data, 0, padded, 0, msgLen);
            padded[msgLen] = 0x80;
            
            var bitLen = (ulong)msgLen * 8;
            for (int i = 0; i < 8; i++)
            {
                padded[56 + i] = (byte)(bitLen >> (i * 8));
            }

            // Process blocks
            for (int offset = 0; offset < padded.Length; offset += 64)
            {
                var x = new uint[16];
                for (int i = 0; i < 16; i++)
                {
                    x[i] = BitConverter.ToUInt32(padded, offset + i * 4);
                }

                var (a, b, c, d) = (state[0], state[1], state[2], state[3]);

                // Round 1
                F(a, b, c, d, x[0], 3, 0x5a827999); F(d, a, b, c, x[1], 7, 0x5a827999); F(c, d, a, b, x[2], 11, 0x5a827999); F(b, c, d, a, x[3], 19, 0x5a827999);
                F(a, b, c, d, x[4], 3, 0x5a827999); F(d, a, b, c, x[5], 7, 0x5a827999); F(c, d, a, b, x[6], 11, 0x5a827999); F(b, c, d, a, x[7], 19, 0x5a827999);
                F(a, b, c, d, x[8], 3, 0x5a827999); F(d, a, b, c, x[9], 7, 0x5a827999); F(c, d, a, b, x[10], 11, 0x5a827999); F(b, c, d, a, x[11], 19, 0x5a827999);
                F(a, b, c, d, x[12], 3, 0x5a827999); F(d, a, b, c, x[13], 7, 0x5a827999); F(c, d, a, b, x[14], 11, 0x5a827999); F(b, c, d, a, x[15], 19, 0x5a827999);

                // Round 2
                G(a, b, c, d, x[0], 3, 0x6ed9eba1); G(d, a, b, c, x[4], 5, 0x6ed9eba1); G(c, d, a, b, x[8], 9, 0x6ed9eba1); G(b, c, d, a, x[12], 13, 0x6ed9eba1);
                G(a, b, c, d, x[1], 3, 0x6ed9eba1); G(d, a, b, c, x[5], 5, 0x6ed9eba1); G(c, d, a, b, x[9], 9, 0x6ed9eba1); G(b, c, d, a, x[13], 13, 0x6ed9eba1);
                G(a, b, c, d, x[2], 3, 0x6ed9eba1); G(d, a, b, c, x[6], 5, 0x6ed9eba1); G(c, d, a, b, x[10], 9, 0x6ed9eba1); G(b, c, d, a, x[14], 13, 0x6ed9eba1);
                G(a, b, c, d, x[3], 3, 0x6ed9eba1); G(d, a, b, c, x[7], 5, 0x6ed9eba1); G(c, d, a, b, x[11], 9, 0x6ed9eba1); G(b, c, d, a, x[15], 13, 0x6ed9eba1);

                // Round 3
                H(a, b, c, d, x[0], 3, 0x8fbbc00); H(d, a, b, c, x[8], 9, 0x8fbbc00); H(c, d, a, b, x[4], 11, 0x8fbbc00); H(b, c, d, a, x[12], 15, 0x8fbbc00);
                H(a, b, c, d, x[2], 3, 0x8fbbc00); H(d, a, b, c, x[10], 9, 0x8fbbc00); H(c, d, a, b, x[6], 11, 0x8fbbc00); H(b, c, d, a, x[14], 15, 0x8fbbc00);
                H(a, b, c, d, x[1], 3, 0x8fbbc00); H(d, a, b, c, x[9], 9, 0x8fbbc00); H(c, d, a, b, x[5], 11, 0x8fbbc00); H(b, c, d, a, x[13], 15, 0x8fbbc00);
                H(a, b, c, d, x[3], 3, 0x8fbbc00); H(d, a, b, c, x[11], 9, 0x8fbbc00); H(c, d, a, b, x[7], 11, 0x8fbbc00); H(b, c, d, a, x[15], 15, 0x8fbbc00);

                state[0] = (state[0] + a) & 0xffffffff;
                state[1] = (state[1] + b) & 0xffffffff;
                state[2] = (state[2] + c) & 0xffffffff;
                state[3] = (state[3] + d) & 0xffffffff;
            }

            var result = new byte[16];
            for (int i = 0; i < 4; i++)
            {
                BitConverter.GetBytes(state[i]).CopyTo(result, i * 4);
            }
            return result;
        }

        private static void F(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            a = (a + ((b ^ c ^ d) + x + t)) & 0xffffffff;
            a = (a << s) | (a >> (32 - s));
            a = (a + b) & 0xffffffff;
        }

        private static void G(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            a = (a + (((b & c) | (~b & d)) + x + t)) & 0xffffffff;
            a = (a << s) | (a >> (32 - s));
            a = (a + b) & 0xffffffff;
        }

        private static void H(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            a = (a + ((b ^ c ^ d) + x + t)) & 0xffffffff;
            a = (a << s) | (a >> (32 - s));
            a = (a + b) & 0xffffffff;
        }

        private static byte[] HMACMD5(byte[] key, byte[] data)
        {
            using (var hmac = new HMACMD5(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        private static byte[] RC4Crypt(byte[] key, byte[] data)
        {
            var state = new byte[256];
            for (int i = 0; i < 256; i++) state[i] = (byte)i;

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + state[i] + key[i % key.Length]) % 256;
                (state[i], state[j]) = (state[j], state[i]);
            }

            var output = new byte[data.Length];
            int x = 0, y = 0;
            for (int i = 0; i < data.Length; i++)
            {
                x = (x + 1) % 256;
                y = (y + state[x]) % 256;
                (state[x], state[y]) = (state[y], state[x]);
                output[i] = (byte)(data[i] ^ state[(state[x] + state[y]) % 256]);
            }
            return output;
        }

        private static bool ArrayEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}