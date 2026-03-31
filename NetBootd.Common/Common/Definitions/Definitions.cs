/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Netboot.Common.Common.Definitions
{
    /// <summary>
    /// Indicates the type of data in a subblock (LE)
    /// </summary>
    public enum NTLMSSPSubBlockTypes : ushort
    {
        Terminator = 0,
        ServerName = 1,
        DomainName = 2,
        DNSHostname = 3,
        DNSDomainName = 4,
        DnsTreeName = 5,
        Flags = 6,
        Timestamp = 7,
        SingleHost = 8,
        TargetName = 9,
        ChannelBindings = 10
    }

    public enum ntlmssp_message_type : uint
    {
        Negotiate = 1,
        Challenge = 2,
        Authenticate = 3
    }

    /// <summary>
    /// Little Endian
    /// </summary>
    [Flags]
    public enum ntlmssp_flags : uint
    {
        /// <summary>
        /// This flag is set to indicate that the server/client will be using UNICODE strings. (Server <-> Client)
        /// </summary>
        UNICODE = 1,
        /// <summary>
        /// This flag is set to indicate that the server/client will be using OEM strings. (Server <-> Client)
        /// </summary>
        OEM = 2,
        /// <summary>
        /// If set, a TargetName field of the CHALLENGE_MESSAGE MUST be supplied.
        /// </summary>
        REQUEST_TARGET = 4,
        /// <summary>
        /// requests session key negotiation for message signatures. If the client sends SIGN 
        /// to the server in the NEGOTIATE_MESSAGE, the server MUST return SIGN to the client in the CHALLENGE_MESSAGE.
        /// </summary>
        SIGN = 16,
        /// <summary>
        /// requests session key negotiation for message confidentiality. If the client sends SEAL
        /// to the server in the NEGOTIATE_MESSAGE, the server MUST return SEAL to the client in the CHALLENGE_MESSAGE.
        /// </summary>
        SEAL = 32,
        /// <summary>
        /// requests connectionless authentication. If DATAGRAM is set, then KEY_EXCH MUST 
        /// always be set in the AUTHENTICATE_MESSAGE to the server and the CHALLENGE_MESSAGE to the client.
        /// </summary>
        DATAGRAM = 64,
        /// <summary>
        /// requests LAN Manager (LM) session key computation.
        /// </summary>
        LM_KEY = 128,
        /// <summary>
        /// Netware 
        /// </summary>
        NETWARE = 256,
        /// <summary>
        /// Indicates that NTLM authentication is supported. (Server <-> Client)
        /// </summary>
        NTLM = 512,
        /// <summary>
        /// requests only NT session key computation
        /// </summary>
        NT_ONLY = 1024,
        /// <summary>
        /// The connection SHOULD be anonymous
        /// </summary>
        ANONYMOUS = 2048,
        /// <summary>
        /// The domain name is provided.
        /// </summary>
        OEM_DOMAIN_SUPPLIED = 4096,
        /// <summary>
        /// This flag indicates whether the Workstation field is present.
        /// </summary>
        OEM_WORKSTATION_SUPPLIED = 8192,
        /// <summary>
        /// The server sets this flag to inform the client that the server and client are on the same machine.
        /// The server provides a local security context handle with the message.
        /// </summary>
        LOCAL_CALL = 16384,
        /// <summary>
        /// requests the presence of a signature block on all messages. ALWAYS_SIGN MUST
        /// be set in the NEGOTIATE_MESSAGE to the server and the CHALLENGE_MESSAGE to the client.
        /// </summary>
        ALWAYS_SIGN = 32768,
        /// <summary>
        /// TargetName MUST be a domain name. The data corresponding to this flag is
        /// provided by the server in the TargetName field of the CHALLENGE_MESSAGE.
        /// </summary>
        TARGET_TYPE_DOMAIN = 65536,
        /// <summary>
        /// If set, TargetName MUST be a server name. The data corresponding to this flag is provided
        /// by the server in the TargetName field of the CHALLENGE_MESSAGE.
        /// </summary>
        TARGET_TYPE_SERVER = 131072,
        /// <summary>
        /// TargetName MUST be a share name. The data corresponding to this flag is
        /// provided by the server in the TargetName field of the CHALLENGE_MESSAGE.
        /// </summary>
        TARGET_TYPE_SHARE = 262144,
        /// <summary>
        /// requests usage of the NTLM v2 session security. 
        /// </summary>
        EXTENDED_SESSIONSECURITY = 524288,
        /// <summary>
        /// Requests an identify level token.
        /// </summary>
        IDENTIFY = 1048576,
        /// <summary>
        /// requests the usage of the LMOWF
        /// </summary>
        REQUEST_NON_NT_SESSION_KEY = 4194304,
        /// <summary>
        /// indicates that the TargetInfo fields in the CHALLENGE_MESSAGE are populated.
        /// </summary>
        TARGET_INFO = 8388608,
        /// <summary>
        /// If set, requests the protocol version number. The data corresponding to this flag
        /// is provided in the Version field of the NEGOTIATE_MESSAGE, the CHALLENGE_MESSAGE,
        /// and the AUTHENTICATE_MESSAGE.
        /// </summary>
        VERSION = 33554432,
        /// <summary>
        /// If the client sends KEY128 to the server in the NEGOTIATE_MESSAGE,
        /// the server MUST return KEY128 to the client in the CHALLENGE_MESSAGE
        /// only if the client sets SEAL or SIGN.
        /// </summary>
        KEY128 = 536870912,
        /// <summary>
        /// This requests an explicit key exchange. This capability SHOULD be used because it improves security for message integrity or confidentiality.
        /// </summary>
        KEY_EXCH = 1073741824,
        /// <summary>
        /// If the client sends SEAL or SIGN with KEY56 to the server in the NEGOTIATE_MESSAGE,
        /// the server MUST return KEY56 to the client in the CHALLENGE_MESSAGE.
        /// </summary>
        KEY56 = 2147483648
    }

    public enum OSPlatformId : byte
    {
        Windows = 0,
        Linux = 1,
        MacOS = 2,
        Ios = 3,
        Android = 4,
        FreeBSD = 5
    }

    public enum EndianessBehavier : byte
    {
        LittleEndian,
        BigEndian
    }

}
