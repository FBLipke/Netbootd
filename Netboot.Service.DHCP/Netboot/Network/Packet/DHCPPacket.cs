using Netboot.Common.Netboot.Common;
using Netboot.Network.Definitions;
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

		public BOOTPOPCode BootpOPCode
		{
			get => (BOOTPOPCode)Read_UINT8();
			set => Write_UINT8(Convert.ToByte(value));
		}

		public PXEVendorID GetVendorIdent
		{
			get
			{
				var vendorId = PXEVendorID.None;

				if (Options.ContainsKey(60))
				{
					var option = GetOption(60);
					
					if (option == null)
						return vendorId;

					var ident = option.Data.GetString().Trim().Split(':', 1).FirstOrDefault();
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
				Buffer.Position = 28;
				return Read_Bytes(HardwareLength);
			}
			set
			{
				Buffer.Position = 28;
				Write_Bytes(value, 16);
			}
		}

		public string ServerName
		{
			get
			{
				Buffer.Position = 44;
				return Encoding.ASCII.GetString(Read_Bytes(64));
			}
			set
			{
				Buffer.Position = 44;
				Write_Bytes(Encoding.ASCII.GetBytes(value), 64);
			}
		}

		public string FileName
		{
			get
			{
				Buffer.Position = 108;
				return Encoding.ASCII.GetString(Read_Bytes(128));
			}
			set
			{
				Buffer.Position = 108;
				Write_Bytes(Encoding.ASCII.GetBytes(value), 128);
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

				var opt = (byte)Buffer.ReadByte();

				if (opt != byte.MaxValue)
				{
					Buffer.Position = i + 1;
					var len = Buffer.ReadByte();
					var data = new byte[len];

					Buffer.Position = i + 2;
					Buffer.Read(data, 0, len);

					AddOption(new(opt, data));

					i += 2 + len;
				}
				else
				{
					AddOption(new(opt));
					break;
				}
			}

			Options.OrderBy(key => key.Key);
			Buffer.Position = curPos;

			if (HasOption(77) && GetOption(77).Data.GetString().Contains("PXE"))
			{
				Console.WriteLine("[W] Option 77: Non RFC compilant option data! (iPXE)");
			}
		}

		public DHCPPacket CreateResponse(IPAddress serverIP)
		{
			DHCPPacket packet = null;
			var msgType = (DHCPMessageType)GetOption(53).Data[0];

			switch (BootpOPCode)
			{
				default:
				case BOOTPOPCode.BootRequest:
					packet = new(ServiceType);
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

					packet.AddOption(new(54, packet.ServerIP));

					var opt97 = GetOption(97);
					if (opt97 != null)
						packet.AddOption(opt97);

					switch (GetVendorIdent)
					{
						case PXEVendorID.PXEClient:
							packet.AddOption(new(60, "PXEClient", Encoding.ASCII));
							break;
						case PXEVendorID.PXEServer:
							packet.AddOption(new(60, "PXEClient", Encoding.ASCII));
							break;
						case PXEVendorID.AAPLBSDPC:
							packet.AddOption(new(60, "APPLBSDPC", Encoding.ASCII));
							break;
						case PXEVendorID.None:
						case PXEVendorID.Msft:
						default:
							break;
					}
					packet.AddOption(new(60, "PXEClient", Encoding.ASCII));

					switch (msgType)
					{
						case DHCPMessageType.Discover:
							packet.AddOption(new(53, (byte)DHCPMessageType.Offer));
							break;
						case DHCPMessageType.Request:
						case DHCPMessageType.Inform:
							packet.AddOption(new(53, (byte)DHCPMessageType.Ack));
							break;
						default:
							break;
					}
					break;
				case BOOTPOPCode.BootReply:
					packet = new DHCPPacket(ServiceType);
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
				// Write Option number
				offset += Write_UINT8(option.Option, offset);
				Buffer.Position = offset;
				if (option.Option == byte.MaxValue)
					break;

				// Write Option length
				offset += Write_UINT8(option.Length,offset);
				Buffer.Position = offset;

				// Write Option data
				if (option.Length != 1)
				{
					Buffer.Write(option.Data);
					offset += option.Length;
				}
				else
					offset += Write_UINT8(Convert.ToByte(option.Data[0]), offset);

				Buffer.Position = offset;
			}

			Buffer.SetLength(offset);
			Buffer.Capacity = offset;
			Buffer.Position = curPosition;
		}
	}
}
