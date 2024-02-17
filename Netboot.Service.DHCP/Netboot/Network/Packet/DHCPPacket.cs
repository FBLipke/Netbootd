using Netboot.Common;
using Netboot.Network.Definitions;
using System.Buffers.Binary;
using System.Linq;
using System.Net;
using System.Text;

namespace Netboot.Network.Packet
{
	public class DHCPPacket : BasePacket
	{
		public Dictionary<byte, DHCPOption> Options { get; } = [];

		public DHCPPacket(string serviceType, byte[] data)
			: base(serviceType, data)
		{
			ParsePacket();
		}

		public DHCPPacket(string serviceType) : base(serviceType)
		{
		}

		public DHCPPacket(string serviceType, int length) : base(serviceType, length)
		{
		}

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
				return new List<DHCPOption>();

			for (var i = 0; i < optionData.Length;)
			{
				var o = optionData[i];

				if (o != byte.MaxValue)
				{
					var len = optionData[i + 1];
					var data = new byte[len];

					Array.Copy(optionData, (i + 2), data, 0, len);

					dict.Add(new DHCPOption(o, data));

					i += 2 + len;
				}
				else
				{
					dict.Add(new DHCPOption(o));
					break;
				}
			}

			return dict;
		}

		public PXEVendorID GetVendorIdent
		{
			get
			{
				var vendorId = PXEVendorID.None;

				if (Options.ContainsKey((byte)DHCPOptions.Vendorclassidentifier))
				{
					var option = GetOption((byte)DHCPOptions.Vendorclassidentifier);
					
					if (option == null)
						return vendorId;

					var identStr = option.Data.GetString().Trim();
					var delim = new char[] { ':' };

					if (identStr.Contains(':'))
						delim = [':'];
					else if (identStr.Contains(' '))
						delim = [' '];

					var ident = identStr.Split(delim).FirstOrDefault();

					if (!string.IsNullOrEmpty(ident))
						Enum.TryParse(ident, out vendorId);
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
				Buffer.Position = 4;
				return Read_UINT32();
			}
			set
			{
				Buffer.Position = 4;
				Write_UINT32(value);
			}
		}

		public ushort Seconds
		{
			get
			{
				Buffer.Position = 8;
				return Read_UINT16();
			}
			set
			{
				Buffer.Position = 8;
				Write_UINT16(value);
			}
		}

		public ushort Flags
		{
			get
			{
				Buffer.Position = 10;
				return Read_UINT16();
			}
			set
			{
				Buffer.Position = 10;
				Write_UINT16(value);
			}
		}

		public IPAddress ClientIP
		{
			get
			{
				Buffer.Position = 12;
				return Read_IPAddress();
			}
			set
			{
				Buffer.Position = 12;
				Write_IPAddress(value);
			}
		}

		public IPAddress YourIP
		{
			get
			{
				Buffer.Position = 16;
				return Read_IPAddress();
			}
			set
			{
				Buffer.Position = 16;
				Write_IPAddress(value);
			}
		}

		public IPAddress ServerIP
		{
			get
			{
				Buffer.Position = 20;
				return Read_IPAddress();
			}
			set
			{
				Buffer.Position = 20;
				Write_IPAddress(value);
			}
		}

		public IPAddress GatewayIP
		{
			get
			{
				Buffer.Position = 24;
				return Read_IPAddress();
			}
			set
			{
				Buffer.Position = 24;
				Write_IPAddress(value);
			}
		}

		public byte[] HardwareAddress
		{
			get
			{
				var curPOS = Buffer.Position;
				var mac = new byte[HardwareLength];
				Buffer.Position = 28;
				Buffer.Read(mac, 0, mac.Length);
				Buffer.Position = curPOS;

				return mac;
			}
			set
			{
				var curPOS = Buffer.Position;

				var mac = new byte[16];
				Array.Copy(value, 0, mac, 0, value.Length);

				Buffer.Position = 28;
				Write_Bytes(mac);

				Buffer.Position = curPOS;
			}
		}

		public string ServerName
		{
			get
			{
				var curPOS = Buffer.Position;
				Buffer.Position = 44;
				var result = Encoding.ASCII.GetString(Read_Bytes(64));
				Buffer.Position = curPOS;

				return result;
			}
			set
			{
				var curPos = Buffer.Position;
				var serverName = Encoding.ASCII.GetBytes(value);
				var bytes = new byte[64];
				Array.Copy(serverName, 0, bytes, 0, serverName.Length);

				Buffer.Position = 44;
				Write_Bytes(bytes);

				Buffer.Position = curPos;
			}
		}

		public string FileName
		{
			get
			{
				var curPOS = Buffer.Position;

				Buffer.Position = 108;
				var result = Encoding.ASCII.GetString(Read_Bytes(128));
				Buffer.Position = curPOS;

				return result;
			}
			set
			{
				var curPos = Buffer.Position;
				var fileName = Encoding.ASCII.GetBytes(value);
				var bytes = new byte[128];
				Array.Copy(fileName, 0, bytes, 0, fileName.Length);

				Buffer.Position = 108;
				Write_Bytes(fileName);
				Buffer.Position = curPos;
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
			var curPos = Buffer.Position;
			Options.Clear();
			var cookieoffset = 240;

			for (var i = cookieoffset; i < Buffer.Length;)
			{
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

					i += 2 + len;
				}
				else
				{
					// Options like 255
					AddOption(new(opt));
					break;
				}
			}

			Options.OrderBy(key => key.Key);
			Buffer.Position = curPos;
		}

		public DHCPPacket CreateResponse(IPAddress serverIP)
		{
			DHCPPacket packet = null;
			var msgType = (DHCPMessageType)GetOption((byte)DHCPOptions.DHCPMessageType).Data[0];

			var expectedLength = BinaryPrimitives.ReadUInt16LittleEndian
				(GetOption((byte)DHCPOptions.MaximumDHCPMessageSize).Data);

			switch (BootpOPCode)
			{
				default:
				case BOOTPOPCode.BootRequest:
					packet = new(ServiceType, expectedLength);
					packet.ServerName = Environment.MachineName;
					packet.HardwareType = HardwareType;
					packet.HardwareLength = HardwareLength;
					packet.Hop = Hop;
					packet.TransactionId = TransactionId;
					packet.Seconds = Seconds;
					packet.Flags = Flags;
					packet.ClientIP = ClientIP;
					packet.YourIP = YourIP;
					packet.ServerIP = serverIP;
					packet.GatewayIP = GatewayIP;
					packet.BOOTPVendor = BOOTPVendor;
					packet.BootpOPCode = BOOTPOPCode.BootReply;
					packet.HardwareAddress = HardwareAddress;

					packet.AddOption(new((byte)DHCPOptions.ServerIdentifier, packet.ServerIP));

					switch (GetVendorIdent)
					{
						case PXEVendorID.PXEClient:
							packet.AddOption(new((byte)DHCPOptions.Vendorclassidentifier, "PXEClient", Encoding.ASCII));
							break;
						case PXEVendorID.PXEServer:
							packet.AddOption(new((byte)DHCPOptions.Vendorclassidentifier, "PXEServer", Encoding.ASCII));
							break;
						case PXEVendorID.AAPLBSDPC:
							packet.AddOption(new((byte)DHCPOptions.Vendorclassidentifier, "APPLBSDPC", Encoding.ASCII));
							break;
						case PXEVendorID.None:
						case PXEVendorID.Msft:
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

			if (!Options.ContainsKey(byte.MaxValue))
				AddOption(new (byte.MaxValue));

			var length = 0;

			foreach (var option in Options.Values)
				length += option.Option != byte.MaxValue ? 2 + option.Length : 1;

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
