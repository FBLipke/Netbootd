using Netboot.Network.Interfaces;
using System.Net;
using System.Runtime.CompilerServices;

namespace Netboot.Network.Packet
{
	public abstract class BasePacket : IPacket
	{
		public MemoryStream Buffer { get; set; }

		public string ServiceType { get; set; }

		public BasePacket() { }

		public BasePacket(string serviceType, byte[] data)
		{
			Buffer = new MemoryStream(data);
			ServiceType = serviceType;
		}

		public BasePacket(string serviceType)
		{
			Buffer = new MemoryStream();
			ServiceType = serviceType;
		}

		public void SetCapacity(int capacity)
		{
			Buffer.Capacity = capacity;
		}

		public void Dispose()
		{
		}

		public byte Read_UINT8(long position = 0)
		{
			if (position != 0)
				Buffer.Position = position;
			else
				Buffer.Position = 0;

			return Convert.ToByte(Buffer.ReadByte());
		}

		public int Write_UINT8(byte value, long position = 0)
		{
			if (position != 0)
				Buffer.Position = position;
			else
				Buffer.Position = 0;

			Buffer.WriteByte(value);

			return sizeof(byte);
		}

		public byte[] Read_Bytes(long size)
		{
			var bytes = new byte[size];
			Buffer.Read(bytes, 0, bytes.Length);

			return bytes;
		}

		public void Write_Bytes(byte[] input, int length = 0)
		{
			var buffer = new byte[length != 0 ? length : input.Length];
			Array.Copy(input, 0, buffer, 0, input.Length);

			Buffer.Write(buffer, 0, buffer.Length);
		}

		public IPAddress Read_IPAddress() => new IPAddress(Read_Bytes(IPAddress.None.GetAddressBytes().Length));

		public void Write_IPAddress(IPAddress address) => Write_Bytes(address.GetAddressBytes());

		public ushort Read_UINT16() => BitConverter.ToUInt16(Read_Bytes(2));

		public void Write_UINT16(ushort value) => Write_Bytes(BitConverter.GetBytes(value));

		public uint Read_UINT32() => BitConverter.ToUInt32(Read_Bytes(sizeof(uint)));

		public void Write_UINT32(uint value) => Write_Bytes(BitConverter.GetBytes(value));
	}
}
