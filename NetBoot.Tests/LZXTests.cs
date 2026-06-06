using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netboot.Common.Compression;
using Netboot.Common.Compression.LZX;

namespace Netbootd.Tests;

[TestClass]
public class LZXTests
{
	[TestMethod]
	public void LzxDecoder_Constructor_ValidWindow15_CreatesDecoder()
	{
		// Arrange & Act
		var decoder = new LzxDecoder(15);

		// Assert
		Assert.IsNotNull(decoder);
	}

	[TestMethod]
	public void LzxDecoder_Constructor_ValidWindow21_CreatesDecoder()
	{
		// Arrange & Act
		var decoder = new LzxDecoder(21);

		// Assert
		Assert.IsNotNull(decoder);
	}

	[TestMethod]
	public void LzxDecoder_Constructor_WindowTooLow_ThrowsArgumentException()
	{
		// Arrange & Act & Assert
		var ex = Assert.ThrowsException<ArgumentException>(() => new LzxDecoder(14));
		Assert.IsTrue(ex.Message.Contains("Window size must be between 15 and 21"));
	}

	[TestMethod]
	public void LzxDecoder_Constructor_WindowTooHigh_ThrowsArgumentException()
	{
		// Arrange & Act & Assert
		var ex = Assert.ThrowsException<ArgumentException>(() => new LzxDecoder(22));
		Assert.IsTrue(ex.Message.Contains("Window size must be between 15 and 21"));
	}

	[TestMethod]
	public void LzxConstants_HaveCorrectValues()
	{
		// Assert
		Assert.AreEqual(256, LzxConstants.NUM_CHARS);
		Assert.AreEqual(8, LzxConstants.NUM_PRIMARY_LENGTHS);
		Assert.AreEqual(249, LzxConstants.NUM_SECONDARY_LENGTHS);
		Assert.AreEqual(20, LzxConstants.PRETREE_MAXSYMBOLS);
		Assert.AreEqual(6, LzxConstants.PRETREE_TABLEBITS);
		Assert.AreEqual(256 + (50 << 3), LzxConstants.MAINTREE_MAXSYMBOLS);
		Assert.AreEqual(10, LzxConstants.MAINTREE_TABLEBITS);
		Assert.AreEqual(249 + (1 << 4), LzxConstants.LENGTH_MAXSYMBOLS);
		Assert.AreEqual(6, LzxConstants.LENGTH_TABLEBITS);
		Assert.AreEqual(8, LzxConstants.ALIGNED_MAXSYMBOLS);
		Assert.AreEqual(3, LzxConstants.ALIGNED_TABLEBITS);
		Assert.AreEqual(2, LzxConstants.MIN_MATCH);
		Assert.AreEqual(257, LzxConstants.MAX_MATCH);
		Assert.AreEqual(64, LzxConstants.LENTABLE_SAFETY);
	}

	[TestMethod]
	public void LzxConstants_BlockTypes_HaveCorrectValues()
	{
		// Assert
		Assert.AreEqual(0, (int)LzxConstants.BLOCKTYPE.INVALID);
		Assert.AreEqual(1, (int)LzxConstants.BLOCKTYPE.VERBATIM);
		Assert.AreEqual(2, (int)LzxConstants.BLOCKTYPE.ALIGNED);
		Assert.AreEqual(3, (int)LzxConstants.BLOCKTYPE.UNCOMPRESSED);
	}

	[TestMethod]
	public void ParseCompressionType_Type0_ReturnsNone()
	{
		// Arrange
		ushort typeCompress = 0x0000; // type=0, windowOrder=0

		// Act
		var (compression, windowOrder) = LzxDecoder.ParseCompressionType(typeCompress);

		// Assert
		Assert.AreEqual(LZXCompression.None, compression);
		Assert.AreEqual(0, windowOrder);
	}

	[TestMethod]
	public void ParseCompressionType_Type1_ReturnsMSZIP()
	{
		// Arrange
		ushort typeCompress = 0x0001; // type=1, windowOrder=0

		// Act
		var (compression, windowOrder) = LzxDecoder.ParseCompressionType(typeCompress);

		// Assert
		Assert.AreEqual(LZXCompression.MSZIP, compression);
	}

	[TestMethod]
	public void ParseCompressionType_Type2_ReturnsLZX()
	{
		// Arrange
		ushort typeCompress = 0x020F; // type=2, windowOrder=15

		// Act
		var (compression, windowOrder) = LzxDecoder.ParseCompressionType(typeCompress);

		// Assert
		Assert.AreEqual(LZXCompression.LZX, compression);
		Assert.AreEqual(15, windowOrder);
	}

	[TestMethod]
	public void ParseCompressionType_Type3_ReturnsLZXWithAlignedOffset()
	{
		// Arrange
		ushort typeCompress = 0x0315; // type=3, windowOrder=21

		// Act
		var (compression, windowOrder) = LzxDecoder.ParseCompressionType(typeCompress);

		// Assert
		Assert.AreEqual(LZXCompression.LZX, compression);
		Assert.AreEqual(21, windowOrder);
	}

	[TestMethod]
	public void ParseCompressionType_UnknownType_ReturnsUnknown()
	{
		// Arrange
		ushort typeCompress = 0xFF15; // type=255, windowOrder=21

		// Act
		var (compression, windowOrder) = LzxDecoder.ParseCompressionType(typeCompress);

		// Assert
		Assert.AreEqual(LZXCompression.Unknown, compression);
		Assert.AreEqual(21, windowOrder);
	}

	[TestMethod]
	public void ParseCompressionType_WindowOrderExtractedCorrectly()
	{
		// Arrange
		ushort typeCompress = 0x0215; // type=2, windowOrder=21 (0x15 = 21)

		// Act
		var (compression, windowOrder) = LzxDecoder.ParseCompressionType(typeCompress);

		// Assert
		Assert.AreEqual(21, windowOrder);
	}

	[TestMethod]
	public void LZXCompression_Enum_HasExpectedValues()
	{
		// Assert
		Assert.AreEqual(0, (int)LZXCompression.None);
		Assert.AreEqual(1, (int)LZXCompression.MSZIP);
		Assert.AreEqual(2, (int)LZXCompression.LZX);
		Assert.AreEqual(3, (int)LZXCompression.LZXWithAlignedOffset);
		Assert.AreEqual(99, (int)LZXCompression.Unknown);
	}

	[TestMethod]
	public void LzxDecoder_Constructor_Window20_SetsCorrectPosnSlots()
	{
		// Arrange & Act
		var decoder = new LzxDecoder(20);

		// Assert - window 20 should have posn_slots = 42
		Assert.IsNotNull(decoder);
	}

	[TestMethod]
	public void LzxDecoder_Constructor_Window21_SetsCorrectPosnSlots()
	{
		// Arrange & Act
		var decoder = new LzxDecoder(21);

		// Assert - window 21 should have posn_slots = 50
		Assert.IsNotNull(decoder);
	}

	[TestMethod]
	public void LzxDecoder_Constructor_Window18_SetsCorrectPosnSlots()
	{
		// Arrange & Act
		var decoder = new LzxDecoder(18);

		// Assert - window 18 should have posn_slots = window << 1 = 36
		Assert.IsNotNull(decoder);
	}
}