using Netboot.Common.Netboot.Common.Definitions;
using Netboot.Network.Interfaces;

namespace Netboot.Common.Netboot.Cryptography
{
	public class NTLMState
	{
		public byte[] NTHash { get; private set; }

		public byte[] LMHash { get; private set; }

		public byte[] Challenge { get; private set; }

		public byte[] LM_Response { get; private set; }

		public byte[] NT_Response { get; private set; }

		public ntlmssp_flags NegotiatedFlags { get; set; }

		public NTLMState() {
		}
	}

	public class NTLMSSP
	{
		public NTLMSSP() {
		}
	}
}
