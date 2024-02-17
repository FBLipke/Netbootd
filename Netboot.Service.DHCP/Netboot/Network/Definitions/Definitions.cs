using Netboot.Common;
using System.Net;

namespace Netboot.Network.Definitions
{
	public enum DHCPHardwareType : byte
	{
		Ethernet = 1,
		IEEE802 = 6,
		ARCNet = 7,
		LocalTalk = 11,
		LocalNet = 12,
		SMDS = 14,
		FrameRelay = 15,
		ATM1 = 16,
		HDLC = 17,
		FireChannel = 18,
		ATM2 = 19,
		SerialLine = 20
	}

	public enum BOOTPVendor : uint
	{
		/// <summary>
		///	The BOOTP Packet has no Vendor specific options set.
		/// </summary>
		DHCP = 1666417251,
	}

	public enum PXEVendorID : byte
	{
		None,
		PXEClient,
		PXEServer,
		AAPLBSDPC,
		Msft,
	}

	public enum PXEVendorEncOptions : byte
	{
		MultiCastIPAddress = 1,
		MulticastClientPort = 2,
		MulticastServerPort = 3,
		MulticastTFTPTimeout = 4,
		MulticastTFTPDelay = 5,
		DiscoveryControl = 6,
		DiscoveryMulticastAddress = 7,
		BootServers = 8,
		BootMenue = 9,
		MenuPrompt = 10,
		MulticastAddressAllocation = 11,
		CredentialTypes = 12,
		BootItem = 71,
		End = byte.MaxValue
	}

	public class BootServer
	{
		public List<IPAddress> Addresses
		{
			get; private set;
		}

		public string Hostname{ get; private set; }
		
		public ushort Type { get; private set; }

		public BootServer(string hostname, BootServerTypes type = BootServerTypes.MicrosoftWindowsNT)
		{
			Type = (ushort)type;
			Hostname = hostname;

			Addresses = Functions.DNSLookup(Environment.MachineName)
				.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
		}

		public enum BootServerTypes : ushort
		{
			PXEBootstrapServer = 0,
			MicrosoftWindowsNT = 1,
			IntelLCM = 2,
			DOSUNDI = 3,
			NECESMPRO = 4,
			IBMWSoD = 5,
			IBMLCCM = 6,
			CAUnicenterTNG = 7,
			HPOpenView = 8,
			Reserved = 9,
			Vendor = 32768,
			ApiTest = ushort.MaxValue
		}

		public BootServer(IPAddress addr, BootServerTypes bootServerType = BootServerTypes.PXEBootstrapServer)
		{
			Hostname = addr.ToString();
			Type = (ushort)bootServerType;

			Addresses = new List<IPAddress>
			{
				addr
			};
		}
	}

	public enum DHCPMessageType
	{
		Discover = 1,
		Offer = 2,
		Request = 3,
		Decline = 4,
		Ack = 5,
		Nak = 6,
		Release = 7,
		Inform = 8,
		ForceRenew = 9,
		LeaseQuery = 10,
		LeaseUnassined = 11,
		LeaseUnknown = 12,
		LeaseActive = 13,
		BulkLeaseQuery = 14,
		LeaseQueryDone = 15,
		ActiveLeaseQuery = 16,
		LeasequeryStatus = 17,
		Tls = 18
	}

	public enum BOOTPOPCode : byte
	{
		BootRequest = 1,
		BootReply = 2
	}

	public enum DHCPOptions : byte
	{
		Pad = byte.MinValue,
		SubnetMask = 1,
		TimeOffset = 2,
		Router = 3,
		TimeServer = 4,
		NameServer = 5,
		DomainNameServer = 6,
		LogServer = 7,
		CookieServer = 8,
		LPRServer = 9,
		ImpressServer = 10,
		ResourceLocServer = 11,
		ClientHostName = 12,
		BootFileSize = 13,
		MeritDump = 14,
		DomainName = 15,
		SwapServer = 16,
		RootPath = 17,
		ExtensionsPath = 18,
		IpForwarding = 19,
		NonLocalSourceRouting = 20,
		PolicyFilter = 21,
		MaximumDatagramReAssemblySize = 22,
		DefaultIPTimeToLive = 23,
		PathMTUAgingTimeout = 24,
		PathMTUPlateauTable = 25,
		InterfaceMTU = 26,
		AllSubnetsAreLocal = 27,
		BroadcastAddress = 28,
		PerformMaskDiscovery = 29,
		MaskSupplier = 30,
		PerformRouterDiscovery = 31,
		RouterSolicitationAddress = 32,
		StaticRoute = 33,
		TrailerEncapsulation = 34,
		ARPCacheTimeout = 35,
		EthernetEncapsulation = 36,
		TCPDefaultTTL = 37,
		TCPKeepaliveInterval = 38,
		TCPKeepaliveGarbage = 39,
		NetworkInformationServiceDomain = 40,
		NetworkInformationServers = 41,
		NetworkTimeProtocolServers = 42,
		VendorSpecificInformation = 43,
		NetBIOSoverTCPIPNameServer = 44,
		NetBIOSoverTCPIPDatagramDistributionServer = 45,
		NetBIOSoverTCPIPNodeType = 46,
		NetBIOSoverTCPIPScope = 47,
		XWindowSystemFontServer = 48,
		XWindowSystemDisplayManager = 49,
		RequestedIPAddress = 50,
		IPAddressLeaseTime = 51,
		OptionOverload = 52,
		DHCPMessageType = 53,
		ServerIdentifier = 54,
		ParameterRequestList = 55,
		Message = 56,
		MaximumDHCPMessageSize = 57,
		RenewalTimeValue_T1 = 58,
		RebindingTimeValue_T2 = 59,
		Vendorclassidentifier = 60,
		ClientIdentifier = 61,
		NetworkInformationServicePlusDomain = 64,
		NetworkInformationServicePlusServers = 65,
		TFTPServerName = 66,
		BootfileName = 67,
		MobileIPHomeAgent = 68,
		SMTPServer = 69,
		POP3Server = 70,
		NNTPServer = 71,
		DefaultWWWServer = 72,
		DefaultFingerServer = 73,
		DefaultIRCServer = 74,
		StreetTalkServer = 75,
		STDAServer = 76,
		UserClass = 77,
		Architecture = 93,
		ClientInterfaceIdent = 94,
		GUID = 97,
		VOIPTFTPServer = 120,

		#region "PXELinux"
		MAGICOption = 208,
		ConfigurationFile = 209,
		PathPrefix = 210,
		RebootTime = 211,
		#endregion

		#region "Windows Deployment Server"
		WDSNBP = 250,
		BCDPath = 252,
		#endregion

		End = byte.MaxValue
	}

	public enum Architecture : ushort
	{
		x86PC = 0,
		NECPC98 = 1,
		EFIItanium = 2,
		DECAlpha = 3,
		Arcx86 = 4,
		IntelLeanClient = 5,
		EFI_IA32 = 6,
		EFIByteCode = 7,
		EFI_xScale = 8,
		EFI_x8664 = 9
	}
}
