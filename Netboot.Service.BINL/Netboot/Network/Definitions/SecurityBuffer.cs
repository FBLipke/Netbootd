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

using System.Buffers.Binary;

namespace Netboot.Network.Definitions
{
	public class SecurityBuffer
	{
		public ushort Length { get; private set; }

		public ushort AllocatedLength { get; private set; }

		public uint Offset { get; private set; }

		public SecurityBuffer(ushort length, uint offset)
		{ Length = length; Offset = offset; }

		public SecurityBuffer(byte[] buffer)
		{
			var lenBytes = new byte[sizeof(ushort)];
			Array.Copy(buffer, 0, lenBytes, 0, lenBytes.Length);

			Length = AllocatedLength = BinaryPrimitives.ReadUInt16LittleEndian(lenBytes);

			var offsetBytes = new byte[sizeof(uint)];
			Array.Copy(buffer, 0, offsetBytes, 0, offsetBytes.Length);
			Offset = BinaryPrimitives.ReadUInt32LittleEndian(offsetBytes);
		}

	}
}
