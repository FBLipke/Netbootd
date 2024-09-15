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
using Netboot.Network.Definitions;
using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace Netboot.Network.Packet
{
	public class DHCPPacket : BasePacket
	{
		/// <summary>
		/// The offset (DHCP_OPTIONS_START_OFFSET) after the magic cookie, that we can use to parse the dhcp options...
		/// </summary>
		public const byte DHCP_OPTIONS_START_OFFSET = 240;

		public Dictionary<byte, DHCPOption> Options { get; } = [];

		public DHCPPacket() {
			ParsePacket();
		}

		public DHCPPacket(string serviceType, byte[] data)
			: base(serviceType, data) => ParsePacket();

		/// <summary>
		/// Indicates that the packet was relayed
		/// </summary>
		public bool IsRelayed { get => GatewayIP != IPAddress.Any; }

		public DHCPPacket(string serviceType) : base(serviceType) { }

		public DHCPPacket(string serviceType, int length) : base(serviceType, length) {	}

		public BOOTPOPCode BootpOPCode
		{
			get => (BOOTPOPCode)Read_UINT8();
			set => Write_UINT8(Convert.ToByte(value));
		}

		public List<DHCPOption> GetEncOptions(byte opt)
		{
			var dict = new List<DHCPOption>();

			var optionData = GetOption(opt)?.Data;
			if (optionData == null)
				return [];

			for (var i = 0; i < optionData.Length;)
			{
				var o = optionData[i];

				if (o == byte.MaxValue)
				{
					dict.Add(new(o));
					break;
				}
				else
				{
					var len = optionData[i + 1];
					var data = new byte[len];

					Array.Copy(optionData, (i + 2), data, 0, len);
					dict.Add(new(o, data));
					i += 2 + len;
				}
			}

			return dict;
		}

		public DHCPVendorID GetVendorIdent
		{
			get
			{
				var vendorId = DHCPVendorID.None;

				if (HasOption((byte)DHCPOptions.Vendorclassidentifier))
				{
					var option = GetOption((byte)DHCPOptions.Vendorclassidentifier);

					if (option == null)
						return vendorId;

					var identStr = option.Data.GetString().Trim();

					if (identStr.Contains("PXEClient"))
						vendorId = DHCPVendorID.PXEClient;
					else if (identStr.Contains("PXEServer"))
						vendorId = DHCPVendorID.PXEServer;
					else if (identStr.Contains("AAPLBSDPC"))
						vendorId = DHCPVendorID.AAPLBSDPC;
					else if (identStr.Contains("HTTPClient"))
						vendorId = DHCPVendorID.HTTPClient;
					else
					{
						var delim = new char[] { identStr.Contains(':') ? ':' : '/' };
						var ident = identStr.Split(delim).FirstOrDefault();

						if (!string.IsNullOrEmpty(ident))
							Enum.TryParse(ident, out vendorId);
					}
				}

				return vendorId;
			}
		}

		public DHCPHardwareType HardwareType
		{
			get => (DHCPHardwareType)Read_UINT8(1);
			set => Write_UINT8(Convert.ToByte(value), 1);
		}

		public byte HardwareLength
		{
			get => Read_UINT8(2);
			set => Write_UINT8(value, 2);
		}

		public byte Hop
		{
			get => Read_UINT8(3);
			set => Write_UINT8(value, 3);
		}

		public uint TransactionId
		{
			get
			{
				SetPosition(4);
				var result = Read_UINT32();
				RestorePosition();
				return result;
			}
			set
			{
				SetPosition(4);
				Write_UINT32(value);
				RestorePosition();
			}
		}

		public ushort Seconds
		{
			get
			{
				SetPosition(8);
				var result = Read_UINT16();
				RestorePosition();
				return result;
			}
			set
			{
				SetPosition(8);
				Write_UINT16(value);
				RestorePosition();
			}
		}

		public ushort Flags
		{
			get
			{
				SetPosition(16);
				var result = Read_UINT16();
				RestorePosition();

				return result;
			}
			set
			{
				SetPosition(16);
				Write_UINT16(value);
				RestorePosition();
			}
		}

		public IPAddress ClientIP
		{
			get
			{
				SetPosition(12);
				var result = Read_IPAddress();
				RestorePosition();

				return result;
			}
			set
			{
				SetPosition(12);
				Write_IPAddress(value);
				RestorePosition();
			}
		}

		public IPAddress YourIP
		{
			get
			{
				SetPosition(16);
				var result = Read_IPAddress();
				RestorePosition();

				return result;
			}
			set
			{
				SetPosition(16);
				Write_IPAddress(value);
				RestorePosition();
			}
		}

		public IPAddress ServerIP
		{
			get
			{
				SetPosition(20);
				var result = Read_IPAddress();
				RestorePosition();

				return result;
			}
			set
			{
				SetPosition(20);
				Write_IPAddress(value);
				RestorePosition();
			}
		}

		public IPAddress GatewayIP
		{
			get
			{
				SetPosition(24);
				var result = Read_IPAddress();
				RestorePosition();

				return result;
			}
			set
			{
				SetPosition(24);
				Write_IPAddress(value);
				RestorePosition();
			}
		}

		public byte[] HardwareAddress
		{
			get
			{
				SetPosition(28);
				var mac = Read_Bytes(HardwareLength);
				RestorePosition();

				return mac;
			}
			set
			{
				var mac = new byte[16];
				Array.Copy(value, 0, mac, 0, value.Length);

				SetPosition(28);
				Write_Bytes(mac);
				RestorePosition();
			}
		}

		public string ServerName
		{
			get
			{
				SetPosition(44);
				var result = Encoding.ASCII.GetString(Read_Bytes(64));
				RestorePosition();

				return result;
			}
			set
			{

				var serverName = Encoding.ASCII.GetBytes(value);
				var bytes = new byte[64];
				Array.Copy(serverName, 0, bytes, 0, serverName.Length);

				SetPosition(44);
				Write_Bytes(bytes);
				RestorePosition();
			}
		}

		public string FileName
		{
			get
			{
				SetPosition(108);
				var result = Encoding.ASCII.GetString(Read_Bytes(128));
				RestorePosition();

				return result;
			}
			set
			{

				var fileName = Encoding.ASCII.GetBytes(value);
				var bytes = new byte[128];
				Array.Copy(fileName, 0, bytes, 0, fileName.Length);

				SetPosition(108);
				Write_Bytes(fileName);
				RestorePosition();
			}
		}

		public BOOTPVendor BOOTPVendor
		{
			get
			{
				Buffer.Position = 236;
				return (BOOTPVendor)BitConverter.ToUInt32(Read_Bytes(4));
			}
			set
			{
				Buffer.Position = 236;
				Write_Bytes(BitConverter.GetBytes(Convert.ToUInt32(value)));
			}
		}

		public void AddOption(DHCPOption dhcpoption)
		{
			if (dhcpoption == null)
				return;

			if (!Options.TryAdd(dhcpoption.Option, dhcpoption))
				Options[dhcpoption.Option] = dhcpoption;
		}

		public DHCPOption? GetOption(byte opt) => HasOption(opt) ? Options[opt] : null;

		public bool HasOption(byte opt)
			=> Options.ContainsKey(opt);

		void ParsePacket()
		{
			if (Buffer == null)
				return;

			var curPos = Buffer.Position;

			if (Options == null)
				return;
			
			Options.Clear();
			var cookieEndoffset = DHCP_OPTIONS_START_OFFSET;

			for (var i = cookieEndoffset; i < Buffer.Length;)
			{
				#region "Parse DHCP Option"
				Buffer.Position = i;

				// Option
				var opt = (byte)Buffer.ReadByte();

				if (opt != byte.MaxValue)
				{
					// Length
					Buffer.Position = i + 1;
					var len = Buffer.ReadByte();

					// Data					
					var data = new byte[len];
					Buffer.Position = i + 2;
					Buffer.Read(data, 0, len);

					AddOption(new(opt, data));

					i += (byte)(2 + len);
				}
				else
				{
					// Options like 255
					AddOption(new(opt));
					break;
				}
				#endregion
			}

			Options.OrderBy(key => key.Key);
			Buffer.Position = curPos;
		}

		public DHCPPacket CreateResponse(IPAddress serverIP)
		{
			DHCPPacket packet = null;
			var msgType = (DHCPMessageType)GetOption((byte)DHCPOptions.DHCPMessageType).Data[0];

			var expectedLength = HasOption((byte)DHCPOptions.MaximumDHCPMessageSize) ? BinaryPrimitives.ReadUInt16LittleEndian
				(GetOption((byte)DHCPOptions.MaximumDHCPMessageSize).Data) : 0;

			switch (BootpOPCode)
			{
				default:
				case BOOTPOPCode.BootRequest:
                    packet = new(ServiceType, expectedLength)
                    {
                        ServerName = Environment.MachineName,
                        HardwareType = HardwareType,
                        HardwareLength = HardwareLength,
                        Hop = Hop,
                        TransactionId = TransactionId,
                        Seconds = Seconds,
                        Flags = Flags,
                        ClientIP = ClientIP,
                        YourIP = YourIP,
                        ServerIP = serverIP,
                        GatewayIP = GatewayIP,
                        BOOTPVendor = BOOTPVendor,
                        BootpOPCode = BOOTPOPCode.BootReply,
                        HardwareAddress = HardwareAddress
                    };
                    packet.AddOption(new((byte)DHCPOptions.ServerIdentifier, packet.ServerIP));

					switch (GetVendorIdent)
					{
						case DHCPVendorID.PXEClient:
							packet.AddOption(new((byte)DHCPOptions.Vendorclassidentifier, "PXEClient", Encoding.ASCII));
							break;
						case DHCPVendorID.PXEServer:
							packet.AddOption(new((byte)DHCPOptions.Vendorclassidentifier, "PXEServer", Encoding.ASCII));
							break;
						case DHCPVendorID.AAPLBSDPC:
							packet.AddOption(new((byte)DHCPOptions.Vendorclassidentifier, "APPLBSDPC", Encoding.ASCII));
							break;
						default:
							break;
					}

					switch (msgType)
					{
						case DHCPMessageType.Discover:
							packet.AddOption(new((byte)DHCPOptions.DHCPMessageType, (byte)DHCPMessageType.Offer));
							break;
						case DHCPMessageType.Request:
						case DHCPMessageType.Inform:
							packet.AddOption(new((byte)DHCPOptions.DHCPMessageType, (byte)DHCPMessageType.Ack));

							if (Options.ContainsKey(43))
								packet.Options.Add(43, Options[43]);
							break;
						default:
							break;
					}

					var opt97 = GetOption((byte)DHCPOptions.GUID);
					if (opt97 != null)
						packet.AddOption(opt97);

					break;
				case BOOTPOPCode.BootReply:
					packet = new DHCPPacket(ServiceType, expectedLength);
					switch (msgType)
					{
						case DHCPMessageType.Offer:
							break;
						case DHCPMessageType.Ack:
							break;
						default:
							break;
					}
					break;
			}

			return packet;
		}

		public void CommitOptions()
		{
			Options.OrderBy(key => key.Key);

			// add End Option (if needed)
			if (!Options.ContainsKey(byte.MaxValue))
				AddOption(new(byte.MaxValue));

			var curPosition = Buffer.Position;
			var offset = 240;

			Buffer.Position = offset;
			foreach (var option in Options.Values)
			{
				#region "DHCP Option Number"
				offset += Write_UINT8(Convert.ToByte(option.Option), offset);
				Buffer.Position = offset;

				if (option.Option == byte.MaxValue)
					break;
				#endregion

				#region "DHCP Option Length"
				// Write Option length
				offset += Write_UINT8(option.Length, offset);
				Buffer.Position = offset;
				#endregion

				#region "DHCP Option Data"
				// Write Option data
				if (option.Length != 1)
				{
					Buffer.Write(option.Data);
					offset += option.Length;
				}
				else
					offset += Write_UINT8(Convert.ToByte(option.Data.First()), offset);

				Buffer.Position = offset;
				#endregion
			}

			Buffer.SetLength(offset);
			Buffer.Capacity = offset;
			Buffer.Position = curPosition;
		}
	}
}
