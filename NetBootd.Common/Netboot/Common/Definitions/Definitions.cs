using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common.Netboot.Common.Definitions
{
	/// <summary>
	/// Indicates the type of data in a subblock 
	/// </summary>
	public enum NTLMSSPSubBlockTypes : ushort
	{
		Terminator = 0x0000,
		ServerName = 0x0001,
		DomainName = 0x0002,
		DNSHostname = 0x0003,
		DNSDomainName = 0x0004,
		DnsTreeName = 0x0005,
		Flags = 0x0006,
		Timestamp = 0x0007,
		SingleHost = 0x0008,
		TargetName = 0x0009,
		ChannelBindings = 0x000a
	}

	public enum ntlmssp_message_type
	{
		Negotiate = 1,
		Challenge = 2,
		Authenticate = 3,
	}

	[Flags]
	public enum ntlmssp_flags : uint
	{
		/// <summary>
		/// This flag is set to indicate that the server/client will be using UNICODE strings. (Server <-> Client)
		/// </summary>
		UNICODE = 0x00000001,
		/// <summary>
		/// This flag is set to indicate that the server/client will be using OEM strings. (Server <-> Client)
		/// </summary>
		OEM = 0x00000002,
		/// <summary>
		/// If set, a TargetName field of the CHALLENGE_MESSAGE MUST be supplied.
		/// </summary>
		REQUEST_TARGET = 0x00000004,
		/// <summary>
		/// requests session key negotiation for message signatures. If the client sends SIGN 
		/// to the server in the NEGOTIATE_MESSAGE, the server MUST return SIGN to the client in the CHALLENGE_MESSAGE.
		/// </summary>
		SIGN = 0x00000010,
		/// <summary>
		/// requests session key negotiation for message confidentiality. If the client sends SEAL
		/// to the server in the NEGOTIATE_MESSAGE, the server MUST return SEAL to the client in the CHALLENGE_MESSAGE.
		/// </summary>
		SEAL = 0x00000020,
		/// <summary>
		/// requests connectionless authentication. If DATAGRAM is set, then KEY_EXCH MUST 
		/// always be set in the AUTHENTICATE_MESSAGE to the server and the CHALLENGE_MESSAGE to the client.
		/// </summary>
		DATAGRAM = 0x00000040,
		/// <summary>
		/// requests LAN Manager (LM) session key computation.
		/// </summary>
		LM_KEY = 0x00000080,
		/// <summary>
		/// Netware 
		/// </summary>
		NETWARE = 0x00000100,
		/// <summary>
		/// Indicates that NTLM authentication is supported. (Server <-> Client)
		/// </summary>
		NTLM = 0x00000200,
		/// <summary>
		/// requests only NT session key computation
		/// </summary>
		NT_ONLY = 0x00000400,
		/// <summary>
		/// The connection SHOULD be anonymous
		/// </summary>
		ANONYMOUS = 0x00000800,
		/// <summary>
		/// The domain name is provided.
		/// </summary>
		OEM_DOMAIN_SUPPLIED = 0x00001000,
		/// <summary>
		/// This flag indicates whether the Workstation field is present.
		/// </summary>
		OEM_WORKSTATION_SUPPLIED = 0x00002000,
		/// <summary>
		/// The server sets this flag to inform the client that the server and client are on the same machine.
		/// The server provides a local security context handle with the message.
		/// </summary>
		LOCAL_CALL = 0x00004000,
		/// <summary>
		/// requests the presence of a signature block on all messages. ALWAYS_SIGN MUST
		/// be set in the NEGOTIATE_MESSAGE to the server and the CHALLENGE_MESSAGE to the client.
		/// </summary>
		ALWAYS_SIGN = 0x00008000,
		/// <summary>
		/// TargetName MUST be a domain name. The data corresponding to this flag is
		/// provided by the server in the TargetName field of the CHALLENGE_MESSAGE.
		/// </summary>
		TARGET_TYPE_DOMAIN = 0x00010000,
		/// <summary>
		/// If set, TargetName MUST be a server name. The data corresponding to this flag is provided
		/// by the server in the TargetName field of the CHALLENGE_MESSAGE.
		/// </summary>
		TARGET_TYPE_SERVER = 0x00020000,
		/// <summary>
		/// TargetName MUST be a share name. The data corresponding to this flag is
		/// provided by the server in the TargetName field of the CHALLENGE_MESSAGE.
		/// </summary>
		TARGET_TYPE_SHARE = 0x00040000,
		/// <summary>
		/// requests usage of the NTLM v2 session security. 
		/// </summary>
		EXTENDED_SESSIONSECURITY = 0x00080000,
		/// <summary>
		/// Requests an identify level token.
		/// </summary>
		IDENTIFY = 0x00100000,
		/// <summary>
		/// requests the usage of the LMOWF
		/// </summary>
		REQUEST_NON_NT_SESSION_KEY = 0x00400000,
		/// <summary>
		/// indicates that the TargetInfo fields in the CHALLENGE_MESSAGE are populated.
		/// </summary>
		TARGET_INFO = 0x00800000,
		/// <summary>
		/// If set, requests the protocol version number. The data corresponding to this flag
		/// is provided in the Version field of the NEGOTIATE_MESSAGE, the CHALLENGE_MESSAGE,
		/// and the AUTHENTICATE_MESSAGE.
		/// </summary>
		VERSION = 0x02000000,
		/// <summary>
		/// If the client sends KEY128 to the server in the NEGOTIATE_MESSAGE,
		/// the server MUST return KEY128 to the client in the CHALLENGE_MESSAGE
		/// only if the client sets SEAL or SIGN.
		/// </summary>
		KEY128 = 0x20000000,
		/// <summary>
		/// This requests an explicit key exchange. This capability SHOULD be used because it improves security for message integrity or confidentiality.
		/// </summary>
		KEY_EXCH = 0x40000000,
		/// <summary>
		/// If the client sends SEAL or SIGN with KEY56 to the server in the NEGOTIATE_MESSAGE,
		/// the server MUST return KEY56 to the client in the CHALLENGE_MESSAGE.
		/// </summary>
		KEY56 = 0x80000000
	}
}
