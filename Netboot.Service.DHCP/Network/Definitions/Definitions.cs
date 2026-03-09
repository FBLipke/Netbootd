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

namespace Netboot.Service.DHCP.Network.Definitions
{
	public enum Architecture : ushort
	{
		X86PC = 0,
		NECPC98,
		EFIItanium,
		DECAlpha,
		Arcx86,
		IntelLeanClient,
		EFI_IA32,
		EFIByteCode,
		EFI_xScale,
		EFI_x8664,
		ARM32_EFI,
		ARM64_EFI,
		PowerPCOpenFW,
		PowerPCePAPR,
		PowerOpalV3,
		X86EfiHttp,
		X64EfiHttp,
		EfiHttp,
		Arm32EfiHttp,
		Arm64EfiHttp,
		PCBiosHttp,
		Arm32Uboot,
		Arm64UBoot,
		Arm32UbootHttp,
		Arm64UbootHttp,
		RiscV32EFi,
		RiscV32EFiHttp,
		RiscV64EFi,
		RiscV64EFiHttp,
		RiscV128Efi,
		RiscV128EfiHttp,
		S390Basic,
		S390Extended,
		MIPS32Efi,
		MIPS64Efi,
		SunWay32Efi,
		SunWay64Efi,
		LoongArch32Efi,
		LoongArch32EfiHttp,
		LoongArch64Efi,
		LoongArch64EfiHttp,
		ArmRPIBoot
	}

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

	public enum MagicCookie : uint
	{
		/// <summary>
		///	The Packet has only BOOTP specific options set.
		///	(Hint: It has usally only a maximum size of approx 240 bytes)
		/// </summary>
		BOOTP = 0,
		/// <summary>
		/// The Packet has DHCP specific options set (99.130.83.99).
		/// </summary>
		DHCP = 1666417251 
	}

    public enum ServerMode
    {
        AllowAll = 0,
        KnownOnly
    }

    public enum DHCPVendorID : byte
	{
		None,
		PXEClient,
		PXEServer,
		AAPLBSDPC,
		HTTPClient,
		Msft
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
		BootReply
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
		ClientIdGUID = 85,
		SystemArchitecture = 90,
		NeworkSpecifier = 91,
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

		#region "Microsoft WDS / RIS (2003)"
		WDSNBP = 250,
		BCDPath = 252,

		#region "Windows Server 2003 (undocumented PXE loader options)"
		/// <summary>
		/// Example: OSChooser\i386\NTLDR
		/// </summary>
		NTLDRLoaderPath = 251,
		/// <summary>
		/// Example: "boot.ini"
		/// </summary>
		BootIni = 253,
		/// <summary>
		/// OSChooser\i386\["boot.ini" / "or in option "NTLDRLoaderPath" given filename"]
		/// </summary>
		BootIniPath = 254,
		#endregion
		#endregion

		End = byte.MaxValue
	}

	public enum NicSpecType : byte
	{
		UNDI = 1,
		Pci,
		PNP
	}
}
