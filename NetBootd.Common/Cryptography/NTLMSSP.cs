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

using Netboot.Common.Common.Definitions;
using Netboot.Common.Cryptography.Interfaces;

namespace Netboot.Common.Cryptography
{
	public class NTLMState : ICrypto
	{
		public byte[] NTHash { get; private set; }

		public byte[] LMHash { get; private set; }

		public byte[] Challenge { get; private set; }

		public byte[] LM_Response { get; private set; }

		public byte[] NT_Response { get; private set; }

		public ntlmssp_flags NegotiatedFlags { get; set; }

		public NTLMState()
		{
			NTHash = [];
			LMHash = [];
			Challenge = [];
			LM_Response = [];
			NT_Response = [];
		}

		public string GetHash(string text, string key)
		{
			throw new NotImplementedException();
		}

		public byte[] GetHash(byte[] data)
		{
			throw new NotImplementedException();
		}
	}

	public class NTLMSSP
	{
		public NTLMSSP()
		{
		}
	}
}
