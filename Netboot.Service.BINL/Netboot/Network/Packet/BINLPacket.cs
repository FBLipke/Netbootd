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
				SetPosition(0);
				
				var signatureBytes = Read_Bytes(sizeof(uint));
				var result = (BINLMessageTypes)BinaryPrimitives.ReadUInt32BigEndian(signatureBytes);

				RestorePosition();
				return result;
			}
			set
			{
				SetPosition(0);
				
				var signatureBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32BigEndian(signatureBytes, (uint)value);

				Write_Bytes(signatureBytes);
				RestorePosition();
			}
		}


		public NetcardRequestVersion NetcardRequestVersion
		{
			get
			{
				switch (MessageType)
				{
					case BINLMessageTypes.NetcardRequest:
						SetPosition(12);
						break;
					case BINLMessageTypes.NetcardResponse:
						SetPosition(16);
						break;
					default:
						return 0;
				}

				var lenBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(lenBytes);
				RestorePosition();

				return (NetcardRequestVersion)result;
			}
			set
			{
				switch (MessageType)
				{
					case BINLMessageTypes.NetcardRequest:
						SetPosition(12);
						break;
					case BINLMessageTypes.NetcardResponse:
						SetPosition(16);
						break;
					default:
						return;
				}

				var lenBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(lenBytes, (uint)value);

				Write_Bytes(lenBytes);
				RestorePosition();
			}
		}

		/// <summary>
		/// Get or set the Length of the Packet without Length (this) and Tag (OPCode)
		/// </summary>
		public uint Length
		{
			get
			{
				SetPosition(4);
				
				var lenBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(lenBytes);
				RestorePosition();

				return result;
			}

			set
			{
				SetPosition(4);
				
				var lenBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(lenBytes, value);

				Write_Bytes(lenBytes);
				RestorePosition();
			}
		}

		/// <summary>
		/// Get or set the plain NTLMSSP Data out of the Packet.
		/// </summary>
		public NTLMSSPPacket NTLMSSP
		{
			get
			{
				SetPosition(8);

				var ntlmsspBytes = Read_Bytes(Length);

				var result = new NTLMSSPPacket(ServiceType, ntlmsspBytes);
				RestorePosition();

				return result;
			}
		}

		public uint Sequence
		{
			get
			{
				SetPosition(8);

				var seqBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(seqBytes);
				RestorePosition();

				return result;
			}

			set
			{
				SetPosition(8);

				var seqBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(seqBytes, value);

				Write_Bytes(seqBytes);
				RestorePosition();
			}
		}

		public ushort Fragment
		{
			get
			{
				SetPosition(12);

				var fragBytes = Read_Bytes(sizeof(ushort));
				var result = BinaryPrimitives.ReadUInt16LittleEndian(fragBytes);
				RestorePosition();

				return result;
			}

			set
			{
				SetPosition(12);

				var fragBytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16LittleEndian(fragBytes, value);

				Write_Bytes(fragBytes);
				
				RestorePosition();
			}
		}

		public ushort TotalFragments
		{
			get
			{
				SetPosition(14);

				var fragBytes = Read_Bytes(sizeof(ushort));
				var result = BinaryPrimitives.ReadUInt16LittleEndian(fragBytes);
				RestorePosition();

				return result;
			}

			set
			{
				SetPosition(14);

				var fragBytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16LittleEndian(fragBytes, value);

				Write_Bytes(fragBytes);
				RestorePosition();
			}
		}

		public uint Status
		{
			get
			{
				switch (MessageType)
				{
					case BINLMessageTypes.NetcardResponse:
					case BINLMessageTypes.NetcardError:
						SetPosition(12);
						break;
					case BINLMessageTypes.HalResponse:
						SetPosition(8);
						break;
					default:
						RestorePosition();
						return 0;
				}

				var sigLenBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(sigLenBytes);
				RestorePosition();

				return result;
			}

			set
			{
				switch (MessageType)
				{
					case BINLMessageTypes.NetcardResponse:
					case BINLMessageTypes.NetcardError:
						SetPosition(12);
						break;
					case BINLMessageTypes.HalResponse:
						SetPosition(8);
						break;
					default:
						RestorePosition();
						return;
				}

				var sigLenBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(sigLenBytes, value);
				Write_Bytes(sigLenBytes);

				RestorePosition();
			}
		}

		public uint SignLength
		{
			get
			{
				SetPosition(16);

				var sigLenBytes = Read_Bytes(sizeof(uint));
				var result = BinaryPrimitives.ReadUInt32LittleEndian(sigLenBytes);
				RestorePosition();

				switch (MessageType)
				{
					case BINLMessageTypes.RequestSigned:
					case BINLMessageTypes.ResponseSigned:
					case BINLMessageTypes.ErrorSigned:
						return result;
					default:
						return 0;
				}
			}

			set
			{
				SetPosition(16);

				var sigLenBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(sigLenBytes, value);
				switch (MessageType)
				{
					case BINLMessageTypes.RequestSigned:
					case BINLMessageTypes.ResponseSigned:
					case BINLMessageTypes.ErrorSigned:
						Write_Bytes(sigLenBytes);
						break;
					default:
						break;
				}

				RestorePosition();

			}
		}

		public byte[] Sign
		{
			get
			{
				SetPosition(20);

				var signBytes = Read_Bytes(SignLength);

				RestorePosition();

				switch (MessageType)
				{
					case BINLMessageTypes.RequestSigned:
					case BINLMessageTypes.ResponseSigned:
					case BINLMessageTypes.ErrorSigned:
						return signBytes;
					default:
						return [];
				}
			}

			set
			{
				SetPosition(20);
				
				switch (MessageType)
				{
					case BINLMessageTypes.RequestSigned:
					case BINLMessageTypes.ResponseSigned:
					case BINLMessageTypes.ErrorSigned:
						Write_Bytes(value);
						break;
					default:
						break;
				}

				RestorePosition();
			}
		}

		public Common.Definitions.Architecture Architecture
		{
			get
			{

				SetPosition(12);
				var archBytes =  Read_UINT32();
				RestorePosition();

				return (Common.Definitions.Architecture)archBytes;
			}
			set
			{
			
			}
		}

		public Guid Guid
		{
			get
			{
				SetPosition(16);
				var uuidBytes = Read_Bytes(16);
				RestorePosition();

				return new Guid(BitConverter.ToString(uuidBytes).Replace("-", string.Empty));
			}
			set
			{

			}
		}

		public byte[] Data
		{
			get
			{
				SetPosition(36);

				var screenBytes = Read_Bytes(((Length + 8) - Buffer.Position) - 1);
				RestorePosition();

				return screenBytes;
			}

			set
			{
				SetPosition(36);

				Write_Bytes(value);
				RestorePosition();
			}
		}
	}
}
