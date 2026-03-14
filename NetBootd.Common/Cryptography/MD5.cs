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

using Netboot.Common.Cryptography.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Netboot.Common.Cryptography
{
	public class MD5 : ICrypto
	{
		public string GetHash(string text, string key = "notNeededforMD5")
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			var numArray = null as byte[];

			using (var cryptoServiceProvider = global::System.Security.Cryptography.MD5.Create())
				numArray = cryptoServiceProvider.ComputeHash(Encoding.ASCII.GetBytes(text));

			return BitConverter.ToString(numArray).Replace("-", string.Empty).ToLower();
		}

		public byte[] GetHash(byte[] data)
		{
			throw new NotImplementedException();
		}
	}
}
