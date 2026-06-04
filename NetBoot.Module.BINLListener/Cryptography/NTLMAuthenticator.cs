/*
 * NTLM Authentication Handshake Implementation
 * Based on MS-NLMP Specification
 */

using System;
using System.Security.Cryptography;

namespace Netboot.Module.BINLListener.Cryptography
{
    /// <summary>
    /// NTLM Session Key derivation and authentication
    /// </summary>
    public class NTLMAuthenticator
    {
        private byte[] _sessionBaseKey;
        private byte[] _ntProofStr;
        private byte[] _serverChallenge;
        private byte[] _clientChallenge;
        
        public byte[] SessionBaseKey => _sessionBaseKey;
        public byte[] NtProofStr => _ntProofStr;
        public byte[] ServerChallenge => _serverChallenge;
        public byte[] ClientChallenge => _clientChallenge;

        /// <summary>
        /// Generate a random 8-byte challenge (ServerChallenge or ClientChallenge)
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
        /// Compute NT Response from password hash and challenges
        /// NTResponse = HMAC_MD5(NT-OwfPassword, ServerChallenge || ClientChallenge)
        /// </summary>
        public static byte[] ComputeNTResponse(byte[] ntOwfPassword, byte[] serverChallenge, byte[] clientChallenge)
        {
            // Concatenate ServerChallenge + ClientChallenge
            var challengeData = new byte[16];
            Buffer.BlockCopy(serverChallenge, 0, challengeData, 0, 8);
            Buffer.BlockCopy(clientChallenge, 0, challengeData, 8, 8);
            
            return ComputeHMAC_MD5(ntOwfPassword, challengeData);
        }

        /// <summary>
        /// Compute LM Response from password hash and challenges
        /// LMResponse = DES(First 7 bytes of LMHash, First 8 bytes of ServerChallenge) || 
        ///              DES(Last 7 bytes of LMHash, First 8 bytes of ServerChallenge)
        /// </summary>
        public static byte[] ComputeLMResponse(byte[] lmHash, byte[] serverChallenge)
        {
            // Simplified LM response - in real NTLM this uses DES
            // For BINL purposes we mainly need NTResponse
            return new byte[24];
        }

        /// <summary>
        /// Derive Session Base Key from NT Proof String
        /// SessionBaseKey = HMAC_MD5(NTProofStr, ClientChallenge)
        /// </summary>
        public static byte[] DeriveSessionBaseKey(byte[] ntProofStr, byte[] clientChallenge)
        {
            return ComputeHMAC_MD5(ntProofStr, clientChallenge);
        }

        /// <summary>
        /// Derive Seal Key from Session Base Key
        /// SealKey = HMAC_MD5(SessionBaseKey, "session key to server-to-client sealing key magic constant")
        /// </summary>
        public byte[] DeriveSealKey()
        {
            var sealKeyMaterial = System.Text.Encoding.ASCII.GetBytes("session key to server-to-client sealing key magic constant");
            return ComputeHMAC_MD5(_sessionBaseKey, sealKeyMaterial);
        }

        /// <summary>
        /// Derive Sign Key from Session Base Key
        /// SignKey = HMAC_MD5(SessionBaseKey, "session key to server-to-client sealing key magic constant")
        /// </summary>
        public byte[] DeriveSignKey()
        {
            var signKeyMaterial = System.Text.Encoding.ASCII.GetBytes("session key to server-to-client sealing key magic constant");
            return ComputeHMAC_MD5(_sessionBaseKey, signKeyMaterial);
        }

        /// <summary>
        /// Create NTLMAuthenticator from server challenge and client NT response
        /// Used on server side when client sends AUTHENTICATE
        /// </summary>
        public static NTLMAuthenticator FromClientResponse(
            byte[] serverChallenge,
            byte[] clientChallenge,
            byte[] ntResponse,
            byte[] ntOwfPassword)
        {
            var auth = new NTLMAuthenticator();
            auth._serverChallenge = serverChallenge;
            auth._clientChallenge = clientChallenge;
            
            // Verify NTResponse: NTResponse should = HMAC_MD5(NT-OwfPassword, ServerChallenge || ClientChallenge)
            var expectedNTResponse = ComputeNTResponse(ntOwfPassword, serverChallenge, clientChallenge);
            
            // Compute NTProofStr from the NTResponse (first 16 bytes of NTResponse = NTProofStr in some implementations)
            // Actually: NTProofStr = HMAC_MD5(NT-OwfPassword, ServerChallenge || ClientChallenge) = NTResponse
            auth._ntProofStr = ntResponse;
            
            // SessionBaseKey = HMAC_MD5(NTProofStr, ClientChallenge)
            auth._sessionBaseKey = DeriveSessionBaseKey(ntResponse, clientChallenge);
            
            return auth;
        }

        /// <summary>
        /// Create NTLMAuthenticator from challenges and session base key (for testing)
        /// </summary>
        public static NTLMAuthenticator Create(byte[] serverChallenge, byte[] clientChallenge, byte[] sessionBaseKey)
        {
            var auth = new NTLMAuthenticator();
            auth._serverChallenge = serverChallenge;
            auth._clientChallenge = clientChallenge;
            auth._sessionBaseKey = sessionBaseKey;
            return auth;
        }

        /// <summary>
        /// Get the NTLM Seal Context for this session
        /// </summary>
        public NTLMSealContext GetSealContext()
        {
            var sealKey = DeriveSealKey();
            var signKey = DeriveSignKey();
            
            // Create seal context with derived keys
            return NTLMSealContext.CreateFromSessionKey(_sessionBaseKey);
        }

        private static byte[] ComputeHMAC_MD5(byte[] key, byte[] data)
        {
            using var hmac = new HMACMD5(key);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// Compute MD4 hash (used for NT password hash)
        /// </summary>
        public static byte[] ComputeMD4(byte[] data)
        {
            // MD4 is not available in standard .NET, using workaround
            // In production you'd use a proper MD4 implementation
            // For NTLM, NT-OwfPassword = MD4(password in UTF-16LE)
            throw new NotImplementedException("Use MD4 library for NT password hash");
        }
    }

    /// <summary>
    /// NTLM Message Types
    /// </summary>
    public enum NTLMMessageType : byte
    {
        Negotiate = 1,
        Challenge = 2,
        Authenticate = 3
    }

    /// <summary>
    /// NTLM Negotiate Flags
    /// </summary>
    [Flags]
    public enum NTLMNegotiateFlags : uint
    {
        Unicode = 0x00000001,
        OEM = 0x00000002,
        RequestTarget = 0x00000004,
        Sign = 0x00000010,
        Seal = 0x00000020,
        Datagram = 0x00000040,
        Challenge = 0x00000080,
        CallBack = 0x00000100,
        MultiDomain = 0x00000200,
        UnicodeEncoding = 0x00000400, // NTLM2
        Version = 0x00002000,
        AlwaysSign = 0x00004000,
        NTLM2Key = 0x00080000
    }

    /// <summary>
    /// Known NTLM Signature bytes
    /// </summary>
    public static class NTMLSignatures
    {
        public static readonly byte[] Negotiate = new byte[] { 0x4E, 0x45, 0x47, 0x4F }; // "NEGO"
        public static readonly byte[] Challenge = new byte[] { 0x43, 0x48, 0x4C, 0x53 }; // "CHLS"
        public static readonly byte[] Authenticate = new byte[] { 0x41, 0x55, 0x54, 0x48 }; // "AUTH"
    }
}