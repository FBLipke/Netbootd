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

using Netboot.Network.Interfaces;
using System.Net;

namespace Netboot.Network.Packet
{
	public abstract class BasePacket : IPacket
	{
		public MemoryStream Buffer { get; set; }

		long lastPosition { get; set; }

		public string ServiceType { get; set; } = string.Empty;

		public BasePacket()
		{
			Buffer = new();
		}

		public BasePacket(string serviceType, byte[] data)
		{
			Buffer = new(data);
			ServiceType = serviceType;
		}

		public BasePacket(string serviceType)
		{
			Buffer = new();
			ServiceType = serviceType;
		}

		public BasePacket(string serviceType, int length)
		{
			Buffer = new(length);
			ServiceType = serviceType;
		}

		public void SetCapacity(int capacity)
		{
			Buffer.Capacity = capacity;
		}

		public void Dispose() => GC.SuppressFinalize(this);

		public void SetPosition(long position)
		{
			lastPosition = Buffer.Position;
			Buffer.Position = position;
		}

		public void RestorePosition() => Buffer.Position = lastPosition;

		public byte Read_UINT8(long position = 0)
		{
			var curPos = Buffer.Position;

			Buffer.Position = position != 0 ? position : 0;
			var result = Convert.ToByte(Buffer.ReadByte());
			Buffer.Position = curPos;

			return result;
		}

		public int Write_UINT8(byte value, long position = 0)
		{
			Buffer.Position = position != 0 ? position : 0;
			Buffer.WriteByte(value);

			return sizeof(byte);
		}

		public byte[] Read_Bytes(long size)
		{
			var bytes = new byte[size];
			Buffer.Read(bytes, 0, bytes.Length);

			return bytes;
		}

		public int Write_Bytes(byte[] input)
		{
			Buffer.Write(input, 0, input.Length);

			return input.Length;
		}

		public IPAddress Read_IPAddress() => new(Read_Bytes(IPAddress.None.GetAddressBytes().Length));

		public void Write_IPAddress(IPAddress address) => Write_Bytes(address.GetAddressBytes());

		public ushort Read_UINT16() => BitConverter.ToUInt16(Read_Bytes(2));

		public void Write_UINT16(ushort value)
		{
			var bytes = BitConverter.GetBytes(value);
			Write_Bytes(bytes);
		}

		public uint Read_UINT32() => BitConverter.ToUInt32(Read_Bytes(sizeof(uint)));

		public void Write_UINT32(uint value) => Write_Bytes(BitConverter.GetBytes(value));
	}
}
