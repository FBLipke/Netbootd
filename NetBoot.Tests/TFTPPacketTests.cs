using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netboot.Module.TFTPServer;
using System.IO;

namespace Netbootd.Tests;

[TestClass]
public class TFTPPacketTests
{
	private static string _tftpRrqPath = null!;
	private static string _tftpAckPath = null!;
	private static string _tftpDatPath = null!;
	private static string _tftpErrPath = null!;

	[ClassInitialize]
	public static void Setup(TestContext context)
	{
		var baseDir = AppDomain.CurrentDomain.BaseDirectory;
		_tftpRrqPath = Path.Combine(baseDir, "TestData", "tftp_rrq.bin");
		_tftpAckPath = Path.Combine(baseDir, "TestData", "tftp_ack.bin");
		_tftpDatPath = Path.Combine(baseDir, "TestData", "tftp_dat.bin");
		_tftpErrPath = Path.Combine(baseDir, "TestData", "tftp_err.bin");
	}

	#region "Binary File Tests"

	[TestMethod]
	public void TFTP_RRQ_Binary_Exists()
	{
		Assert.IsTrue(File.Exists(_tftpRrqPath), $"TFTP RRQ binary not found at: {_tftpRrqPath}");
	}

	[TestMethod]
	public void TFTP_ACK_Binary_Exists()
	{
		Assert.IsTrue(File.Exists(_tftpAckPath), $"TFTP ACK binary not found at: {_tftpAckPath}");
	}

	[TestMethod]
	public void TFTP_DAT_Binary_Exists()
	{
		Assert.IsTrue(File.Exists(_tftpDatPath), $"TFTP DAT binary not found at: {_tftpDatPath}");
	}

	[TestMethod]
	public void TFTP_ERR_Binary_Exists()
	{
		Assert.IsTrue(File.Exists(_tftpErrPath), $"TFTP ERR binary not found at: {_tftpErrPath}");
	}

	[TestMethod]
	public void TFTP_RRQ_Binary_ParsesCorrectly()
	{
		if (!File.Exists(_tftpRrqPath)) Assert.Inconclusive("TFTP RRQ binary not found");

		var data = File.ReadAllBytes(_tftpRrqPath);
		var packet = new TFTPPacket(data);

		Assert.AreEqual(TFTPOPCodes.RRQ, packet.TFTPOPCode);
		Assert.AreEqual("bootfile.bin", packet.Options["file"]);
		Assert.AreEqual("octet", packet.Options["mode"]);
	}

	[TestMethod]
	public void TFTP_RRQ_Binary_HasBlksizeOption()
	{
		if (!File.Exists(_tftpRrqPath)) Assert.Inconclusive("TFTP RRQ binary not found");

		var data = File.ReadAllBytes(_tftpRrqPath);
		var packet = new TFTPPacket(data);

		Assert.IsTrue(packet.Options.ContainsKey("blksize"));
		Assert.AreEqual("1456", packet.Options["blksize"]);
	}

	[TestMethod]
	public void TFTP_RRQ_Binary_HasTsizeOption()
	{
		if (!File.Exists(_tftpRrqPath)) Assert.Inconclusive("TFTP RRQ binary not found");

		var data = File.ReadAllBytes(_tftpRrqPath);
		var packet = new TFTPPacket(data);

		Assert.IsTrue(packet.Options.ContainsKey("tsize"));
		Assert.AreEqual("0", packet.Options["tsize"]);
	}

	[TestMethod]
	public void TFTP_ACK_Binary_ParsesCorrectly()
	{
		if (!File.Exists(_tftpAckPath)) Assert.Inconclusive("TFTP ACK binary not found");

		var data = File.ReadAllBytes(_tftpAckPath);
		var packet = new TFTPPacket(data);

		Assert.AreEqual(TFTPOPCodes.ACK, packet.TFTPOPCode);
		Assert.AreEqual((ushort)1, packet.Block);
	}

	[TestMethod]
	public void TFTP_DAT_Binary_ParsesCorrectly()
	{
		if (!File.Exists(_tftpDatPath)) Assert.Inconclusive("TFTP DAT binary not found");

		var data = File.ReadAllBytes(_tftpDatPath);
		var packet = new TFTPPacket(data);

		Assert.AreEqual(TFTPOPCodes.DAT, packet.TFTPOPCode);
		Assert.AreEqual((ushort)1, packet.Block);
	}

	[TestMethod]
	public void TFTP_DAT_Binary_HasCorrectData()
	{
		if (!File.Exists(_tftpDatPath)) Assert.Inconclusive("TFTP DAT binary not found");

		var data = File.ReadAllBytes(_tftpDatPath);
		var packet = new TFTPPacket(data);

		var expectedData = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x54, 0x46, 0x54, 0x50, 0x21 };
		CollectionAssert.AreEqual(expectedData, packet.Data);
	}

	[TestMethod]
	public void TFTP_ERR_Binary_ParsesCorrectly()
	{
		if (!File.Exists(_tftpErrPath)) Assert.Inconclusive("TFTP ERR binary not found");

		var data = File.ReadAllBytes(_tftpErrPath);
		var packet = new TFTPPacket(data);

		Assert.AreEqual(TFTPOPCodes.ERR, packet.TFTPOPCode);
		Assert.AreEqual(TFTPErrorCode.FileNotFound, packet.ErrorCode);
		Assert.AreEqual("File not found", packet.ErrorMessage.TrimEnd('\0'));
	}

	#endregion

	#region "In-Memory Tests"

	[TestMethod]
	public void Constructor_RRQ_CreatesPacketWithOpCode()
	{
		var packet = new TFTPPacket(TFTPOPCodes.RRQ);
		Assert.AreEqual(TFTPOPCodes.RRQ, packet.TFTPOPCode);
	}

	[TestMethod]
	public void Constructor_ByteArray_ParsesRRQ()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78, 0x74,
			0x00,
			0x6f, 0x63, 0x74, 0x65, 0x74,
			0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual(TFTPOPCodes.RRQ, packet.TFTPOPCode);
		Assert.AreEqual("test.txt", packet.Options["file"]);
		Assert.AreEqual("octet", packet.Options["mode"]);
	}

	[TestMethod]
	public void TFTPOPCode_GetSet_RoundTrips()
	{
		var packet = new TFTPPacket(TFTPOPCodes.RRQ);
		packet.TFTPOPCode = TFTPOPCodes.ACK;
		Assert.AreEqual(TFTPOPCodes.ACK, packet.TFTPOPCode);
	}

	[TestMethod]
	public void Block_GetSet_RoundTrips()
	{
		var packet = new TFTPPacket(TFTPOPCodes.ACK);
		packet.Block = 123;
		Assert.AreEqual((ushort)123, packet.Block);
	}

	[TestMethod]
	public void Block_ReadFromDAT_ReturnsCorrectValue()
	{
		var data = new byte[] {
			0x00, 0x03,
			0x00, 0x05,
			0x48, 0x65, 0x6c, 0x6c, 0x6f
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual((ushort)5, packet.Block);
	}

	[TestMethod]
	public void Data_ReadFromDAT_ReturnsDataBytes()
	{
		var data = new byte[] {
			0x00, 0x03,
			0x00, 0x01,
			0x48, 0x65, 0x6c, 0x6c, 0x6f
		};
		var packet = new TFTPPacket(data);
		CollectionAssert.AreEqual(new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f }, packet.Data);
	}

	[TestMethod]
	public void Data_SetOnDAT_WritesDataToBuffer()
	{
		var packet = new TFTPPacket(TFTPOPCodes.DAT);
		packet.Block = 1;
		packet.Data = new byte[] { 0x54, 0x65, 0x73, 0x74 };
		// 2 opcode + 2 block + 4 data = 8 bytes
		Assert.AreEqual(8, packet.Buffer.Length);
	}

	[TestMethod]
	public void ErrorCode_GetSet_RoundTrips()
	{
		var packet = new TFTPPacket(TFTPOPCodes.ERR);
		packet.ErrorCode = TFTPErrorCode.FileNotFound;
		
		// Both read and write now use big-endian (RFC 1350)
		Assert.AreEqual(TFTPErrorCode.FileNotFound, packet.ErrorCode);
	}

	[TestMethod]
	public void ErrorMessage_GetSet_RoundTrips()
	{
		var packet = new TFTPPacket(TFTPOPCodes.ERR);
		packet.ErrorCode = TFTPErrorCode.FileNotFound;
		packet.ErrorMessage = "File not found";
		
		// ErrorMessage includes null terminator, so trim it
		Assert.AreEqual("File not found", packet.ErrorMessage.TrimEnd('\0'));
	}

	[TestMethod]
	public void NextWindow_GetSet_RoundTrips()
	{
		var packet = new TFTPPacket(TFTPOPCodes.ACK);
		packet.NextWindow = 2;
		Assert.AreEqual((byte)2, packet.NextWindow);
	}

	[TestMethod]
	public void ParsePacket_RRQWithBlksize_ExtractsBlksizeOption()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78, 0x74, 0x00,
			0x6f, 0x63, 0x74, 0x65, 0x74, 0x00,
			0x62, 0x6c, 0x6b, 0x73, 0x69, 0x7a, 0x65, 0x00,
			0x31, 0x34, 0x34, 0x30, 0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual("1440", packet.Options["blksize"]);
	}

	[TestMethod]
	public void ParsePacket_RRQWithTsize_ExtractsTsizeOption()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78, 0x74, 0x00,
			0x6f, 0x63, 0x74, 0x65, 0x74, 0x00,
			0x74, 0x73, 0x69, 0x7a, 0x65, 0x00,
			0x31, 0x32, 0x33, 0x34, 0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual("1234", packet.Options["tsize"]);
	}

	[TestMethod]
	public void ParsePacket_RRQWithWindowsize_ExtractsWindowsizeOption()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78, 0x74, 0x00,
			0x6f, 0x63, 0x74, 0x65, 0x74, 0x00,
			0x77, 0x69, 0x6e, 0x64, 0x6f, 0x77, 0x73, 0x69, 0x7a, 0x65, 0x00,
			0x36, 0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual("6", packet.Options["windowsize"]);
	}

	[TestMethod]
	public void ParsePacket_RRQWithLeadingSlash_StripsLeadingSlash()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x2f, 0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78, 0x74, 0x00,
			0x6f, 0x63, 0x74, 0x65, 0x74, 0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual("test.txt", packet.Options["file"]);
	}

	[TestMethod]
	public void ParsePacket_RRQWithBackslash_StripsBackslash()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x5c, 0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78, 0x74, 0x00,
			0x6f, 0x63, 0x74, 0x65, 0x74, 0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual("test.txt", packet.Options["file"]);
	}

	[TestMethod]
	public void ParsePacket_ACKWithExtraData_AddsNextWindowOption()
	{
		var data = new byte[] {
			0x00, 0x04,
			0x00, 0x01,
			0x01
		};
		var packet = new TFTPPacket(data);
		Assert.IsTrue(packet.Options.ContainsKey("NextWindow"));
	}

	[TestMethod]
	public void ParsePacket_EmptyRRQ_HandlesGracefully()
	{
		var data = new byte[] {
			0x00, 0x01,
			0x00
		};
		var packet = new TFTPPacket(data);
		Assert.AreEqual(TFTPOPCodes.RRQ, packet.TFTPOPCode);
	}

	[TestMethod]
	public void Options_CanBeModified()
	{
		var packet = new TFTPPacket(TFTPOPCodes.RRQ);
		packet.Options["custom"] = "value";
		Assert.AreEqual("value", packet.Options["custom"]);
	}

	[TestMethod]
	public void CommitOptions_OCK_SetsBufferPosition()
	{
		var packet = new TFTPPacket(TFTPOPCodes.OCK);
		packet.Options["blksize"] = "1456";
		packet.Options["tsize"] = "0";
		packet.CommitOptions();
		Assert.IsTrue(packet.Buffer.Length > 0);
	}

	#endregion
}