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

using System.Net;
using System.Text;

namespace Netboot.Network.Definitions
{
	public class DHCPOption
	{
		public byte Option { get; private set; }
		public byte Length { get; private set; }
		public byte[] Data { get; private set; }

		public DHCPOption(byte option)
		{
			Option = option;
			Length = 0;
			Data = null;
		}

		public DHCPOption(byte option, byte[] data)
		{
			Option = option;
			Length = Convert.ToByte(data.Length);
			Data = data;
		}

		public DHCPOption(byte option, byte data)
		{
			Option = option;
			Length = 1;
			Data = new byte[Length];
			Data[0] = data;
		}

		public DHCPOption(byte option, List<DHCPOption> list)
		{
			var length = 0;

			foreach (var item in list)
				length += item.Option != byte.MaxValue ? 2 + item.Length : 1;

			var offset = 0;
			var block = new byte[length];

			foreach (var item in list)
			{
				block[offset] = item.Option;
				offset += sizeof(byte);

				if (item.Option == byte.MaxValue)
					break;

				if (item.Length == 0)
					continue;

				block[offset] = item.Length;
				offset += sizeof(byte);

				Array.Copy(item.Data, 0, block, offset, item.Data.Length);
				offset += item.Data.Length;
			}

			Option = option;
			Data = block;

			Length = Convert.ToByte(length);
		}

		public DHCPOption(byte option, short data)
		{
			Option = option;
			Data = BitConverter.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, ushort data)
		{
			Option = option;
			Data = BitConverter.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, int data)
		{
			Option = option;
			Data = BitConverter.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, uint data)
		{
			Option = option;
			Data = BitConverter.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, long data)
		{
			Option = option;
			Data = BitConverter.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, ulong data)
		{
			Option = option;
			Data = BitConverter.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, string data, Encoding encoding)
		{
			Option = option;
			Data = encoding.GetBytes(data);
			Length = Convert.ToByte(Data.Length);
		}

		public DHCPOption(byte option, IPAddress data)
		{
			Option = option;
			Data = data.GetAddressBytes();
			Length = Convert.ToByte(Data.Length);
		}
	}
}
