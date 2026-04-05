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

namespace Netboot.Module.DHCPListener
{
    public enum BootpFlags
    {
        Unicast = 0,
        Broadcast = 0x8000
    }

    public enum DHCPHardwareType : byte
    {
        Ethernet = 1,
        ExperiMentalEthernet = 2,
        AmateurRadioAX25 = 3,
        ProteonProNetTokenRing = 4,
        Chaos = 5,
        IEEE802 = 6,
        ARCNet = 7,
        HyperChannel = 8,
        Lanstar = 9,
        AutoNetShortAddr = 10,
        LocalTalk = 11,
        LocalNet = 12,
        UltraLink = 13,
        SMDS = 14,
        FrameRelay = 15,
        ATM1 = 16,
        HDLC = 17,
        FireChannel = 18,
        ATM2 = 19,
        SerialLine = 20,
        ATM3 = 21,
        MILSTD188220 = 22,
        Metricom = 23,
        IEEE1394 = 24,
        MAPOS = 25,
        TWINAxial = 26,
        EUI64 = 27,
        HIARP = 28
    }

    public enum BootServerType : ushort
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
        AppleLegacy = ushort.MaxValue - 5,
        AppleBootServer = ushort.MaxValue - 4,
        LinuxBootServer = ushort.MaxValue - 3,
        BootIntegrityService = ushort.MaxValue - 2,
        WindowsDeploymentServer = ushort.MaxValue - 1,
        ApiTest = ushort.MaxValue
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

    public enum ClientIdentType
    {
        UUID = 0
    }

    public enum DHCPServerMode
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
        AppleMacintosh,
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
        Pad = 0,
        SubnetMask = 1,
        TimeOffset = 2,
        Router = 3,
        TimeServer = 4,
        NameServer = 5,
        DomainNameServer = 6,
        LogServer = 7,
        CookieServer = 8,
        LprServer = 9,
        ImpressServer = 10,
        ResourceLocationServer = 11,
        HostName = 12,
        BootFileSize = 13,
        MeritDumpFile = 14,
        DomainName = 15,
        SwapServer = 16,
        RootPath = 17,
        ExtensionsPath = 18,

        IpForwarding = 19,
        NonLocalSourceRouting = 20,
        PolicyFilter = 21,
        MaximumDatagramReassemblySize = 22,
        DefaultIpTtl = 23,
        PathMtuAgingTimeout = 24,
        PathMtuPlateauTable = 25,

        InterfaceMtu = 26,
        AllSubnetsAreLocal = 27,
        BroadcastAddress = 28,
        PerformMaskDiscovery = 29,
        MaskSupplier = 30,
        PerformRouterDiscovery = 31,
        RouterSolicitationAddress = 32,
        StaticRoute = 33,

        TrailerEncapsulation = 34,
        ArpCacheTimeout = 35,
        EthernetEncapsulation = 36,

        TcpDefaultTtl = 37,
        TcpKeepaliveInterval = 38,
        TcpKeepaliveGarbage = 39,

        NetworkInformationServiceDomain = 40,
        NetworkInformationServers = 41,
        NetworkTimeProtocolServers = 42,
        VendorSpecificInformation = 43,
        NetBiosOverTcpIpNameServer = 44,
        NetBiosOverTcpIpDatagramDistributionServer = 45,
        NetBiosOverTcpIpNodeType = 46,
        NetBiosOverTcpIpScope = 47,
        XWindowSystemFontServer = 48,
        XWindowSystemDisplayManager = 49,

        RequestedIpAddress = 50,
        IpAddressLeaseTime = 51,
        OptionOverload = 52,
        MessageType = 53,
        ServerIdentifier = 54,
        ParameterRequestList = 55,
        Message = 56,
        MaximumDhcpMessageSize = 57,
        RenewalTimeValue = 58,
        RebindingTimeValue = 59,
        VendorClassIdentifier = 60,
        ClientIdentifier = 61,

        NetWareIpDomainName = 62,
        NetWareIpInformation = 63,
        NetworkInformationServicePlusDomain = 64,
        NetworkInformationServicePlusServers = 65,
        TftpServerName = 66,
        BootfileName = 67,
        MobileIpHomeAgent = 68,
        SimpleMailTransportProtocolServer = 69,
        PostOfficeProtocolServer = 70,
        NetworkNewsTransportProtocolServer = 71,
        DefaultWorldWideWebServer = 72,
        DefaultFingerServer = 73,
        DefaultInternetRelayChatServer = 74,
        StreetTalkServer = 75,
        StreetTalkDirectoryAssistanceServer = 76,

        UserClass = 77,
        SlpDirectoryAgent = 78,
        SlpServiceScope = 79,
        RapidCommit = 80,
        FullyQualifiedDomainName = 81,
        RelayAgentInformation = 82,
        InternetStorageNameService = 83,
        NetworkInformationServiceServers = 85,
        NetworkInformationServicePlusServersAlt = 86,
        NdsServers = 87,
        NdsTreeName = 88,
        NdsContext = 89,
        ClientLastTransactionTime = 91,
        AssociatedIp = 92,
        SystemArchitectureType = 93,
        NetworkInterfaceIdentifier = 94,
        ClientMachineIdentifier = 97,
        UuidGuidBasedClientIdentifier = 97,
        UserAuth = 98,
        GeoconfCivic = 99,
        PCode = 100,
        TCode = 101,
        NetInfoAddress = 112,
        NetInfoTag = 113,
        AutoConfigure = 116,
        NameServiceSearch = 117,
        SubnetSelection = 118,
        DomainSearch = 119,
        SipServers = 120,
        ClasslessStaticRoute = 121,
        CableLabsClientConfiguration = 122,
        GeoConfOption = 123,
        VendorIdentifyingVendorClass = 124,
        VendorIdentifyingVendorSpecific = 125,
        Pxe128 = 128,
        Pxe129 = 129,
        Pxe130 = 130,
        Pxe131 = 131,
        Pxe132 = 132,
        Pxe133 = 133,
        Pxe134 = 134,
        Pxe135 = 135,
        MobilityDomain = 136,
        SipUaConfigurationServiceDomains = 137,
        #region "PXELinux"
        MAGICOption = 208,
        ConfigurationFile = 209,
        PathPrefix = 210,
        RebootTime = 211,
        #endregion
        #region "Microsoft WDS / RIS (2003)"
        /// <summary>
        /// WDS Encapsulated Options (like 43...)
        /// </summary>
        WDSNBP = 250,
        /// <summary>
        /// Example: OSChooser\i386\NTLDR
        /// </summary>
        NTLDRLoaderPath = 251,
        BCDPath = 252,
        /// <summary>
        /// Example: "boot.ini"
        /// </summary>
        BootIni = 253,
        /// <summary>
        /// OSChooser\i386\["boot.ini" / "or in option "NTLDRLoaderPath" given filename"]
        /// </summary>
        BootIniPath = 254,
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
