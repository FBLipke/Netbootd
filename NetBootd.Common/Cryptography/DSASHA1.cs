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

namespace Netboot.Common.Cryptography
{
	public class DSASHA1 : IDisposable, ICrypto
	{
		private SHA1 sha1;
		public DSASHA1() => sha1 = SHA1.Create();

		public void Dispose()
		{
			Clear();
			sha1.Dispose();
		}

		public byte[] GetHash(byte[] data)
			=> sha1.ComputeHash(data);

		public void Clear() => sha1.Clear();

		public void Transform(byte[] inputBuffer, int inputOffset, int inputCount, out byte[] target, int offset = 0)
		{
			var tgt = new byte[inputCount];
			sha1.TransformBlock(inputBuffer, inputOffset, inputCount, tgt, offset);
			target = tgt;
		}

		public string GetHash(string text, string key)
		{
			throw new NotImplementedException();
		}
	}
}
