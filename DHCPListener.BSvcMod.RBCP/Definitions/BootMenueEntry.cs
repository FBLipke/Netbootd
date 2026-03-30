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
using System.Buffers.Binary;
using System.Text;

namespace Netboot.Module.DHCPListener
{
	public class BootMenueEntry
	{
		public BootServerType Item { get; private set; }

		public string Description { get; private set; }

		public BootMenueEntry(BootServerType item, string desc)
		{
			Item = item;
			Description = desc;
		}

		public byte[] AsBytes(EndianessBehavier endianess = EndianessBehavier.LittleEndian)
		{
			var descBytes = Encoding.ASCII.GetBytes(Description);
			var itemBytes = new byte[sizeof(ushort) + sizeof(byte) + descBytes.Length];
			var index = 0;

			#region "Item"
			switch (endianess)
			{
				case EndianessBehavier.BigEndian:
					BinaryPrimitives.WriteUInt16BigEndian(itemBytes, (ushort)Item);
					break;
				case EndianessBehavier.LittleEndian:
				default:
					BinaryPrimitives.WriteUInt16LittleEndian(itemBytes, (ushort)Item);
					break;
			}

			index += sizeof(ushort);
			#endregion

			#region "Length"
			itemBytes[index] = Convert.ToByte(descBytes.Length);
			index += sizeof(byte);
			#endregion

			#region "Description"

			Array.Copy(descBytes, 0, itemBytes, index, descBytes.Length);
			index += descBytes.Length;
			#endregion

			return itemBytes;
		}
	}
}
