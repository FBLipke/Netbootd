using Netboot.Common;
using Netboot.Network.Interfaces;
using System.Buffers.Binary;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Netboot.Network.Packet
{
	public abstract class BasePacket : IPacket
	{
		public MemoryStream? Buffer { get; set; }

		public string ServiceType { get; set; } = string.Empty;

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

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

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

		public byte[] Read_Bytes(long size, bool swapEndianess = false)
		{
			var bytes = new byte[size];
			Buffer.Read(bytes, 0, bytes.Length);

			if (swapEndianess)
				Array.Reverse(bytes);

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

		public uint Read_UINT32(bool swapEndianess = false) => BitConverter.ToUInt32(Read_Bytes(sizeof(uint)));

		public void Write_UINT32(uint value) => Write_Bytes(BitConverter.GetBytes(value));
	}
}
