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

using Netboot.Common;
using Netboot.Network.Packet;
using System.Buffers.Binary;
using System.Text;

namespace Netboot.Service.TFTP.Netboot.Network.Packet
{
	public class TFTPPacket : BasePacket
	{
		public readonly Dictionary<string, string> Options = [];

		public TFTPPacket(string serviceType, byte[] bufffer) : base(serviceType, bufffer)
		{
			ParsePacket();
		}

		public TFTPPacket(string serviceType, TFTPOPCodes opCode) : base(serviceType)
		{
			ParsePacket();
			TFTPOPCode = opCode;
		}

		public byte[] Data
		{
			get
			{
				switch (TFTPOPCode)
				{
					case TFTPOPCodes.DAT:
						var curPos = Buffer.Position;
						Buffer.Position = 4;
						var result = Read_Bytes(Buffer.Length - Buffer.Position);
						Buffer.Position = curPos;
						return result;
					default:
						return new byte[0];
				}
			}

			set
			{
				switch (TFTPOPCode)
				{
					case TFTPOPCodes.DAT:
						Buffer.Position = 4;
						Write_Bytes(value);
						break;
					default:
						break;
				}
			}
		}

		public TFTPErrorCode ErrorCode
		{
			get
			{
				var curPos = Buffer.Position;
				Buffer.Position = 2;
				var result = BinaryPrimitives.ReadUInt16BigEndian(Read_Bytes(2));
				Buffer.Position = curPos;
				return (TFTPErrorCode)result;
			}
			set
			{
				var curPos = Buffer.Position;
				Buffer.Position = 2;

				var bytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16BigEndian(bytes, (ushort)value);
				Write_Bytes(bytes);

				Buffer.Position = curPos;
			}
		}

		public string ErrorMessage
		{
			get
			{
				var curPos = Buffer.Position;

				Buffer.Position = 4;
				var result = Read_Bytes(Buffer.Length - Buffer.Position).GetString();
				Buffer.Position = curPos;

				return result;
			}
			set
			{
				Buffer.Position = 4;

				var buffer = new byte[value.Length + 1];
				var bytes = Encoding.ASCII.GetBytes(value);
				Array.Copy(bytes, 0, buffer, 0, bytes.Length);
				Buffer.Position += Write_Bytes(buffer);
			}
		}

		public ushort Block
		{
			get
			{
				var curPos = Buffer.Position;
				Buffer.Position = 2;

				var result = BinaryPrimitives.ReadUInt16BigEndian(Read_Bytes(2));

				Buffer.Position = curPos;
				return result;
			}
			set
			{
				var curPos = Buffer.Position;
				Buffer.Position = 2;

				var bytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
				Write_Bytes(bytes);
				Buffer.Position = curPos;
			}
		}

        public byte NextWindow
        {
            get
            {
                SetPosition(4);
                var result = Read_Bytes(1).FirstOrDefault();
				RestorePosition();
				return result;
            }
            set
            {
				SetPosition(4);
                var bytes = new byte[sizeof(byte)];
                bytes[0] = value;

                Write_Bytes(bytes);
				RestorePosition();
            }
        }

        public TFTPOPCodes TFTPOPCode
		{
			get
			{
				var curPos = Buffer.Position;
				Buffer.Position = 0;
				var result = (TFTPOPCodes)BinaryPrimitives.ReadUInt16BigEndian(Read_Bytes(2));
				Buffer.Position = curPos;

				return result;
			}
			set
			{
				var bytes = new byte[sizeof(ushort)];

				var curPos = Buffer.Position;
				Buffer.Position = 0;

				BinaryPrimitives.WriteUInt16BigEndian(bytes, (ushort)value);
				Write_Bytes(bytes);
				Buffer.Position = curPos;
			}
		}

		public void CommitOptions()
		{
			switch (TFTPOPCode)
			{
				case TFTPOPCodes.OCK:
					Buffer.Position = 2;
					var offset = 2;
					foreach (var option in Options)
					{
						var bytes = Encoding.ASCII.GetBytes(option.Key);
						offset += Write_Bytes(bytes) + 1;

						Buffer.Position = offset;

						bytes = Encoding.ASCII.GetBytes(option.Value);
						offset += Write_Bytes(bytes) + 1;
                        Buffer.Position = offset;
                    }

					Buffer.Position = offset;
					break;
				default:
					break;
			}

			Buffer.Capacity = (int)Buffer.Position;
			Buffer.SetLength(Buffer.Capacity);
		}

		public void ParsePacket()
		{
			switch (TFTPOPCode)
			{
				case TFTPOPCodes.RRQ:
					Buffer.Position = 2;
					var parts = Read_Bytes((Buffer.Length - Buffer.Position)).GetString().Split('\0');

					for (var i = 0; i < parts.Length; i++)
					{
						if (i == 0)
						{
							var file = parts[i];
							if (file.StartsWith('\\') || file.StartsWith('/'))
								file = file.Substring(1);

                            if (!Options.ContainsKey("file"))
								Options.Add("file", file);
							else
								Options["file"] = file;
						}

						if (i == 1)
						{
							if (!Options.ContainsKey("mode"))
								Options.Add("mode", parts[i]);
							else
								Options["mode"] = parts[i];
						}

						if (parts[i] == "blksize")
						{
							if (!Options.ContainsKey(parts[i]))
								Options.Add(parts[i], parts[i + 1]);
							else
								Options[parts[i]] = parts[i + 1];
						}

						if (parts[i] == "tsize")
						{
							if (!Options.ContainsKey(parts[i]))
								Options.Add(parts[i], parts[i + 1]);
							else
								Options[parts[i]] = parts[i + 1];
						}

						if (parts[i] == "windowsize")
						{
							if (!Options.ContainsKey(parts[i]))
								Options.Add(parts[i], parts[i + 1]);
							else
								Options[parts[i]] = parts[i + 1];
						}

						if (parts[i] == "msftwindow")
						{
							if (!Options.ContainsKey(parts[i]))
								Options.Add(parts[i], parts[i + 1]);
							else
								Options[parts[i]] = parts[i + 1];
						}
					}
					break;
				case TFTPOPCodes.ACK:
					if (Buffer.Length > 4)
						Options.Add("NextWindow", "");
					break;
				default:
					break;
			}
		}
	}
}
