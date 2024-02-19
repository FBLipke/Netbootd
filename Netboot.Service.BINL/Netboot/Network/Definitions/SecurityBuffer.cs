using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Network.Definitions
{
	public class SecurityBuffer
	{
		public ushort Length { get; private set; }

		public ushort AllocatedLength { get; private set; }

		public uint Offset { get; private set; }
	
		public SecurityBuffer(ushort length, uint offset)
		{  Length = length; Offset = offset; }

		public SecurityBuffer(byte[] buffer)
		{
			var lenBytes = new byte[sizeof(ushort)];
			Array.Copy(buffer, 0, lenBytes, 0,lenBytes.Length);

			Length = AllocatedLength = BinaryPrimitives.ReadUInt16LittleEndian(lenBytes);

			var offsetBytes = new byte[sizeof(uint)];
			Array.Copy(buffer, 0, offsetBytes, 0, offsetBytes.Length);
			Offset = BinaryPrimitives.ReadUInt32LittleEndian(offsetBytes);
		}

	}
}
