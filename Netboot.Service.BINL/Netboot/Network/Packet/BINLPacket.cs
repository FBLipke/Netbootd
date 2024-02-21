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

using Netboot.Network.Definitions;
using System.Buffers.Binary;

namespace Netboot.Network.Packet
{
	public class BINLPacket : BasePacket
	{
		public BINLPacket(string serviceType, byte[] data)
			: base(serviceType, data)
		{
		}

		public BINLPacket(string serviceType, BINLMessageTypes opCode) : base(serviceType)
		{
			MessageType = opCode;
		}

		public BINLMessageTypes MessageType
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 0;

				var signatureBytes = Read_Bytes(sizeof(uint));
				var result = (BINLMessageTypes)BinaryPrimitives
					.ReadUInt32BigEndian(signatureBytes);

				Buffer.Position = curPOS;
				return result;
			}
			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 0;

				var signatureBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32BigEndian(signatureBytes, (uint)value);

				Write_Bytes(signatureBytes);
				Buffer.Position = curPOS;
			}
		}

		public uint Length
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 4;

				var lenBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(lenBytes);
				Buffer.Position = curPOS;

				return result;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 4;

				var lenBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(lenBytes, value);

				Write_Bytes(lenBytes);
				Buffer.Position = curPOS;
			}
		}

		public NTLMSSPPacket NTLMSSP
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 8;
				var ntlmsspBytes = Read_Bytes(Length);

				var result = new NTLMSSPPacket(ServiceType, ntlmsspBytes);
				Buffer.Position = curPOS;

				return result;
			}
		}

		public uint Sequence
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 8;

				var seqBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(seqBytes);
				Buffer.Position = curPOS;

				return result;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 8;

				var seqBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(seqBytes, value);

				Write_Bytes(seqBytes);
				Buffer.Position = curPOS;
			}
		}


		public ushort Fragment
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 12;

				var fragBytes = Read_Bytes(sizeof(ushort));
				var result = BinaryPrimitives.ReadUInt16LittleEndian(fragBytes);
				Buffer.Position = curPOS;

				return result;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 12;

				var fragBytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16LittleEndian(fragBytes, value);

				Write_Bytes(fragBytes);
				Buffer.Position = curPOS;
			}
		}

		public ushort TotalFragments
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 14;

				var fragBytes = Read_Bytes(sizeof(ushort));
				var result = BinaryPrimitives.ReadUInt16LittleEndian(fragBytes);
				Buffer.Position = curPOS;

				return result;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 14;

				var fragBytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16LittleEndian(fragBytes, value);

				Write_Bytes(fragBytes);
				Buffer.Position = curPOS;
			}
		}

		public uint SignLength
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 16;

				var sigLenBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(sigLenBytes);
				Buffer.Position = curPOS;

				return result;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 16;

				var sigLenBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(sigLenBytes, value);

				Write_Bytes(sigLenBytes);
				Buffer.Position = curPOS;
			}
		}

		public byte[] Sign
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 20;

				var signBytes = Read_Bytes(SignLength);
				Buffer.Position = curPOS;

				return signBytes;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 20;

				Write_Bytes(value);
				Buffer.Position = curPOS;
			}
		}

		public byte[] Data
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 36;

				var screenBytes = Read_Bytes(((Length + 8) - Buffer.Position) - 1);
				Buffer.Position = curPOS;

				return screenBytes;
			}

			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 36;

				Write_Bytes(value);
				Buffer.Position = curPOS;
			}
		}
	}
}
