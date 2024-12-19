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

using Netboot.Common.Definitions;

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

		public NTLMState()
		{
			NTHash = Array.Empty<byte>();
			LMHash = Array.Empty<byte>();
			Challenge = Array.Empty<byte>();
			LM_Response = Array.Empty<byte>();
			NT_Response = Array.Empty<byte>();
		}
	}

	public class NTLMSSP
	{
		public NTLMSSP()
		{
		}
	}
}
