using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netboot.Module.DHCPListener;
using System.IO;
using System.Net;

namespace Netbootd.Tests;

[TestClass]
public class DHCPPacketTests
{
	private static string _testCabPath = null!;
	private static string _dhcpBinPath = null!;

	[ClassInitialize]
	public static void Setup(TestContext context)
	{
		var baseDir = AppDomain.CurrentDomain.BaseDirectory;
		_testCabPath = Path.Combine(baseDir, "TestData", "test.cab");
		_dhcpBinPath = Path.Combine(baseDir, "TestData", "dhcp_discover.bin");
	}

	private static byte[] CreateMinimalDHCPDiscoverPacket()
	{
		var packet = new DHCPPacket(1024);
		packet.BootpOPCode = BOOTPOPCode.BootRequest;
		packet.HardwareType = DHCPHardwareType.Ethernet;
		packet.HardwareLength = 6;
		packet.Hop = 0;
		packet.Seconds = 0;
		packet.Flags = BootpFlags.Unicast;
		packet.ClientIP = IPAddress.None;
		packet.YourIP = IPAddress.None;
		packet.ServerIP = IPAddress.None;
		packet.GatewayIP = IPAddress.None;
		packet.MagicCookie = MagicCookie.DHCP;

		// Set minimal hardware address
		packet.HardwareAddress = new HWAddress(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 });

		// Add DHCP Message Type = Discover (53, 1, 1)
		packet.SetMessageType(DHCPMessageType.Discover);

		// Add End option
		packet.AddOption(new DHCPOption<byte>(byte.MaxValue));

		return packet.Buffer.ToArray();
	}

	#region "Binary File Tests"

	[TestMethod]
	public void DHCP_binary_File_Exists()
	{
		Assert.IsTrue(File.Exists(_dhcpBinPath), $"DHCP binary not found at: {_dhcpBinPath}");
	}

	[TestMethod]
	public void DHCP_binary_HasCorrectSize()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");
		Assert.AreEqual(1024, new FileInfo(_dhcpBinPath).Length);
	}

	[TestMethod]
	public void DHCP_binary_ParsesToDHCPPacket()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.IsNotNull(packet);
		Assert.IsNotNull(packet.Options);
	}

	[TestMethod]
	public void DHCP_binary_HasCorrectOpCode()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.AreEqual(BOOTPOPCode.BootRequest, packet.BootpOPCode);
	}

	[TestMethod]
	public void DHCP_binary_HasCorrectHardwareType()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.AreEqual(DHCPHardwareType.Ethernet, packet.HardwareType);
	}

	[TestMethod]
	public void DHCP_binary_HasCorrectHardwareLength()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.AreEqual(6, packet.HardwareLength);
	}

	[TestMethod]
	public void DHCP_binary_HasCorrectMagicCookie()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		// Magic cookie raw value is 0x63825363 = 1669485411
		Assert.AreEqual((uint)0x63825363, (uint)packet.MagicCookie);
	}

	[TestMethod]
	public void DHCP_binary_HasMessageTypeDiscover()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.IsTrue(packet.HasOption(DHCPOptions.MessageType));
		Assert.AreEqual(DHCPMessageType.Discover, packet.GetMessageType());
	}

	[TestMethod]
	public void DHCP_binary_HasPXEClientVendorId()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.AreEqual(DHCPVendorID.PXEClient, packet.GetVendorIdent);
	}

	[TestMethod]
	public void DHCP_binary_HasParameterRequestList()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.IsTrue(packet.HasOption(DHCPOptions.ParameterRequestList));
	}

	[TestMethod]
	public void DHCP_binary_HasMaxMessageSize()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.IsTrue(packet.HasOption(DHCPOptions.MaximumDhcpMessageSize));
	}

	[TestMethod]
	public void DHCP_binary_HasClientUUID()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.IsTrue(packet.HasOption(DHCPOptions.UuidGuidBasedClientIdentifier));
	}

	[TestMethod]
	public void DHCP_binary_HasClientArchitecture()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.IsTrue(packet.HasOption(DHCPOptions.SystemArchitectureType));
	}

	[TestMethod]
	public void DHCP_binary_HasCorrectTransactionId()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		Assert.AreEqual(0x3e5a7b2fu, packet.TransactionId);
	}

	[TestMethod]
	public void DHCP_binary_IsBroadcast()
	{
		if (!File.Exists(_dhcpBinPath)) Assert.Inconclusive("DHCP binary not found");

		var data = File.ReadAllBytes(_dhcpBinPath);
		var packet = new DHCPPacket(data);

		// RFC 951: Broadcast flag is bit 15, value 0x8000
		// Binary stored big-endian, read as little-endian = 128
		Assert.AreEqual((BootpFlags)0x8000, packet.Flags);
	}

	#endregion

	#region "In-Memory Tests"

	[TestMethod]
	public void Constructor_Empty_CreatesEmptyPacket()
	{
		var packet = new DHCPPacket();
		Assert.IsNotNull(packet);
		Assert.IsNotNull(packet.Options);
		Assert.AreEqual(0, packet.Options.Count);
	}

	[TestMethod]
	public void Constructor_ByteArray_ParsesPacket()
	{
		var data = CreateMinimalDHCPDiscoverPacket();
		var packet = new DHCPPacket(data);

		Assert.IsNotNull(packet);
		Assert.AreEqual(BOOTPOPCode.BootRequest, packet.BootpOPCode);
		Assert.AreEqual(DHCPHardwareType.Ethernet, packet.HardwareType);
		Assert.AreEqual(6, packet.HardwareLength);
		Assert.AreEqual(MagicCookie.DHCP, packet.MagicCookie);
	}

	[TestMethod]
	public void BootpOPCode_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.BootpOPCode = BOOTPOPCode.BootReply;
		Assert.AreEqual(BOOTPOPCode.BootReply, packet.BootpOPCode);
	}

	[TestMethod]
	public void HardwareType_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.HardwareType = DHCPHardwareType.Ethernet;
		Assert.AreEqual(DHCPHardwareType.Ethernet, packet.HardwareType);
	}

	[TestMethod]
	public void HardwareLength_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.HardwareLength = 6;
		Assert.AreEqual(6, packet.HardwareLength);
	}

	[TestMethod]
	public void Hop_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.Hop = 1;
		Assert.AreEqual(1, packet.Hop);
	}

	[TestMethod]
	public void TransactionId_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		var expectedId = 0x12345678u;
		packet.TransactionId = expectedId;
		Assert.AreEqual(expectedId, packet.TransactionId);
	}

	[TestMethod]
	public void Seconds_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.Seconds = 100;
		Assert.AreEqual(100, packet.Seconds);
	}

	[TestMethod]
	public void Flags_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.Flags = BootpFlags.Broadcast;
		Assert.AreEqual(BootpFlags.Broadcast, packet.Flags);
	}

	[TestMethod]
	public void ClientIP_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		var ip = IPAddress.Parse("192.168.1.100");
		packet.ClientIP = ip;
		Assert.AreEqual(ip, packet.ClientIP);
	}

	[TestMethod]
	public void YourIP_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		var ip = IPAddress.Parse("192.168.1.101");
		packet.YourIP = ip;
		Assert.AreEqual(ip, packet.YourIP);
	}

	[TestMethod]
	public void ServerIP_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		var ip = IPAddress.Parse("192.168.1.1");
		packet.ServerIP = ip;
		Assert.AreEqual(ip, packet.ServerIP);
	}

	[TestMethod]
	public void GatewayIP_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		var ip = IPAddress.Parse("192.168.1.254");
		packet.GatewayIP = ip;
		Assert.AreEqual(ip, packet.GatewayIP);
	}

	[TestMethod]
	public void MagicCookie_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.MagicCookie = MagicCookie.DHCP;
		Assert.AreEqual(MagicCookie.DHCP, packet.MagicCookie);
	}

	[TestMethod]
	public void HardwareAddress_GetSet_RoundTrips()
	{
		var packet = new DHCPPacket(1024);
		packet.HardwareLength = 6;
		var mac = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
		packet.HardwareAddress = new HWAddress(mac);
		
		// Read it back
		var readMac = packet.HardwareAddress.Address;
		CollectionAssert.AreEqual(mac, readMac.Take(6).ToArray());
	}

	[TestMethod]
	public void IsRelayed_NoGateway_ReturnsFalse()
	{
		var packet = new DHCPPacket(1024);
		packet.GatewayIP = IPAddress.Parse("0.0.0.0");
		Assert.IsFalse(packet.IsRelayed);
	}

	[TestMethod]
	public void IsRelayed_WithGateway_ReturnsTrue()
	{
		var packet = new DHCPPacket(1024);
		packet.GatewayIP = IPAddress.Parse("192.168.1.1");
		Assert.IsTrue(packet.IsRelayed);
	}

	[TestMethod]
	public void AddOption_SingleOption_AddsToDictionary()
	{
		var packet = new DHCPPacket(1024);
		var option = new DHCPOption<byte>((byte)DHCPOptions.MessageType, new byte[] { 1 });
		packet.AddOption(option);
		Assert.IsTrue(packet.HasOption((byte)DHCPOptions.MessageType));
	}

	[TestMethod]
	public void GetOption_ExistingOption_ReturnsOption()
	{
		var packet = new DHCPPacket(1024);
		var option = new DHCPOption<byte>((byte)DHCPOptions.MessageType, new byte[] { 1 });
		packet.AddOption(option);
		var result = packet.GetOption((byte)DHCPOptions.MessageType);
		Assert.IsNotNull(result);
		Assert.AreEqual((byte)DHCPOptions.MessageType, result.Option);
	}

	[TestMethod]
	public void HasOption_ExistingOption_ReturnsTrue()
	{
		var packet = new DHCPPacket(1024);
		packet.AddOption(new DHCPOption<byte>((byte)DHCPOptions.MessageType, new byte[] { 1 }));
		Assert.IsTrue(packet.HasOption(DHCPOptions.MessageType));
	}

	[TestMethod]
	public void HasOption_NonExistingOption_ReturnsFalse()
	{
		var packet = new DHCPPacket(1024);
		Assert.IsFalse(packet.HasOption(DHCPOptions.Router));
	}

	[TestMethod]
	public void SetMessageType_Discover_SetsCorrectOption()
	{
		var packet = new DHCPPacket(1024);
		packet.SetMessageType(DHCPMessageType.Discover);
		Assert.IsTrue(packet.HasOption(DHCPOptions.MessageType));
		Assert.AreEqual(DHCPMessageType.Discover, packet.GetMessageType());
	}

	[TestMethod]
	public void GetMessageType_Request_ReturnsRequest()
	{
		var packet = new DHCPPacket(1024);
		packet.SetMessageType(DHCPMessageType.Request);
		Assert.AreEqual(DHCPMessageType.Request, packet.GetMessageType());
	}

	[TestMethod]
	public void GetVendorIdent_PXEClient_ReturnsPXEClient()
	{
		var packet = new DHCPPacket(1024);
		packet.AddOption(new DHCPOption<byte>((byte)DHCPOptions.VendorClassIdentifier,
			System.Text.Encoding.ASCII.GetBytes("PXEClient")));
		Assert.AreEqual(DHCPVendorID.PXEClient, packet.GetVendorIdent);
	}

	[TestMethod]
	public void GetVendorIdent_HttpClient_ReturnsHTTPClient()
	{
		var packet = new DHCPPacket(1024);
		packet.AddOption(new DHCPOption<byte>((byte)DHCPOptions.VendorClassIdentifier,
			System.Text.Encoding.ASCII.GetBytes("HTTPClient")));
		Assert.AreEqual(DHCPVendorID.HTTPClient, packet.GetVendorIdent);
	}

	[TestMethod]
	public void CreateRequest_ValidServer_CreatesValidPacket()
	{
		var serverIP = IPAddress.Parse("192.168.1.1");
		var packet = DHCPPacket.CreateRequest(serverIP);
		Assert.IsNotNull(packet);
		Assert.AreEqual(BOOTPOPCode.BootReply, packet.BootpOPCode);
		Assert.AreEqual(DHCPHardwareType.Ethernet, packet.HardwareType);
		Assert.IsTrue(packet.HasOption(DHCPOptions.MessageType));
		Assert.IsTrue(packet.HasOption((byte)DHCPOptions.VendorClassIdentifier));
		Assert.IsTrue(packet.HasOption((byte)DHCPOptions.ServerIdentifier));
	}

	[TestMethod]
	public void IsRelayedRequest_WithGatewayIP_ReturnsTrue()
	{
		var packet = new DHCPPacket(1024);
		packet.GatewayIP = IPAddress.Parse("192.168.1.1");
		Assert.IsTrue(packet.IsRelayed);
	}

	[TestMethod]
	public void IsRelayedRequest_WithoutGatewayIP_ReturnsFalse()
	{
		var packet = new DHCPPacket(1024);
		packet.GatewayIP = IPAddress.Any;
		Assert.IsFalse(packet.IsRelayed);
	}

	#endregion
}