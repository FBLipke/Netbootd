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

using Netboot.Common.Netboot.Common.Definitions;
using Netboot.Network.Definitions;
using System.Buffers.Binary;

namespace Netboot.Network.Packet
{
	public class NTLMSSPPacket : BasePacket
	{
		Dictionary<string, SecurityBuffer> SecurityBuffers = [];

		public NTLMSSPPacket(string serviceType, ntlmssp_message_type essageType) : base(serviceType)
		{
			MessageType = essageType;
		}

		public NTLMSSPPacket(string serviceType, byte[] data) : base(serviceType, data)
		{
			var curPOS = Buffer.Position;

			switch (MessageType)
			{
				case ntlmssp_message_type.Challenge:
					Buffer.Position = 12;
					var secBuffer = Read_Bytes(8);
					SecurityBuffers.Add("TargetName", new SecurityBuffer(secBuffer));

					Buffer.Position = 40;
					secBuffer = Read_Bytes(8);
					SecurityBuffers.Add("TargetInfo", new SecurityBuffer(secBuffer));
					break;
				case ntlmssp_message_type.Authenticate:
					break;

				case ntlmssp_message_type.Negotiate:
				default:
					break;
			}

			Buffer.Position = curPOS;
		}

		public ntlmssp_message_type MessageType
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 8;

				var msgTypeBytes = Read_Bytes(sizeof(uint));

				var result = BinaryPrimitives.ReadUInt32LittleEndian(msgTypeBytes);
				Buffer.Position = curPOS;
				return (ntlmssp_message_type)result;
			}
			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 8;

				var msgTypeBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(msgTypeBytes, (uint)value);
				Write_Bytes(msgTypeBytes);
				Buffer.Position = curPOS;
			}
		}

		public byte[] Challenge
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 24;

				var result = Read_Bytes(8);

				Buffer.Position = curPOS;
				return result;
			}
			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 24;

				Write_Bytes(value);
				Buffer.Position = curPOS;
			}
		}

		public byte[] Context
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 32;

				var result = Read_Bytes(8);

				Buffer.Position = curPOS;
				return result;
			}
			set
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 32;

				Write_Bytes(value);
				Buffer.Position = curPOS;
			}
		}


		public ntlmssp_flags Flags
		{
			get
			{
				var curPOS = Buffer.Position;

				switch (MessageType)
				{
					case ntlmssp_message_type.Negotiate:
						Buffer.Position = 12;
						break;
					case ntlmssp_message_type.Challenge:
						Buffer.Position = 20;
						break;
					case ntlmssp_message_type.Authenticate:
						Buffer.Position = 60;
						break;
					default:
						Buffer.Position = 12;
						break;
				}

				var flagsBytes = Read_Bytes(sizeof(uint));

				var result = BinaryPrimitives.ReadUInt32LittleEndian(flagsBytes);
				Buffer.Position = curPOS;
				return (ntlmssp_flags)result;
			}
			set
			{
				var curPOS = Buffer.Position;

				switch (MessageType)
				{
					case ntlmssp_message_type.Negotiate:
						Buffer.Position = 12;
						break;
					case ntlmssp_message_type.Challenge:
						Buffer.Position = 20;
						break;
					case ntlmssp_message_type.Authenticate:
						Buffer.Position = 60;
						break;
					default:
						Buffer.Position = 12;
						break;
				}

				var flagsBytes = new byte[sizeof(uint)];
				BinaryPrimitives.WriteUInt32LittleEndian(flagsBytes, (uint)value);
				Write_Bytes(flagsBytes);
				Buffer.Position = curPOS;
			}
		}
	}
}
