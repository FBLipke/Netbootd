using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netboot.Common.FileFormats;
using Netboot.Common.FileFormats.Cab;
using System.IO;

namespace Netbootd.Tests;

[TestClass]
public class CabTests
{
    private static string _testCabPath = null!;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        _testCabPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "test.cab");
    }

    [TestMethod]
    public void CabFile_Exists_AtExpectedPath()
    {
        // Assert
        Assert.IsTrue(File.Exists(_testCabPath), $"Test CAB not found at: {_testCabPath}");
    }

    [TestMethod]
    public void MSCab_Constructor_OpensCabFile()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        // Act
        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.IsNotNull(cab.Header);
    }

    [TestMethod]
    public void MSCab_Header_HasCorrectSignature()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x4D, 0x53, 0x43, 0x46 }, cab.Header.signature);
    }

    [TestMethod]
    public void MSCab_Header_HasCorrectVersion()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(1, cab.Header.versionMajor);
        Assert.AreEqual(3, cab.Header.versionMinor);
    }

    [TestMethod]
    public void MSCab_Header_HasCorrectFileCount()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(3, cab.Header.cFiles);
    }

    [TestMethod]
    public void MSCab_Header_HasCorrectFolderCount()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(1, cab.Header.cFolders);
    }

    [TestMethod]
    public void MSCab_Header_HasCorrectSetID()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(1, cab.Header.setID);
    }

    [TestMethod]
    public void MSCab_Folders_ContainsOneFolder()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(1, cab.Folders.Count);
    }

    [TestMethod]
    public void MSCab_Folder_IsUncompressed()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(0, cab.Folders[0].typeCompress);
    }

    [TestMethod]
    public void MSCab_Folder_HasCorrectDataBlockCount()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(3, cab.Folders[0].cCFData);
        Assert.AreEqual(3, cab.Folders[0].DataBlocks.Count);
    }

    [TestMethod]
    public void MSCab_FileEntries_ContainsThreeEntries()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual(3, cab.FileEntries.Count);
    }

    [TestMethod]
    public void MSCab_FileEntries_HaveCorrectNames()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert
        Assert.AreEqual("readme.txt", cab.FileEntries[0].szName);
        Assert.AreEqual("info.txt", cab.FileEntries[1].szName);
        Assert.AreEqual("data.txt", cab.FileEntries[2].szName);
    }

    [TestMethod]
    public void MSCab_FileEntries_HaveCorrectSizes()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert - these are the actual sizes of our test content
        Assert.AreEqual((uint)39, cab.FileEntries[0].cbFile);  // readme.txt
        Assert.AreEqual((uint)40, cab.FileEntries[1].cbFile);  // info.txt
        Assert.AreEqual((uint)28, cab.FileEntries[2].cbFile);  // data.txt
    }

    [TestMethod]
    public void MSCab_DataBlocks_HaveMatchingCompressedAndUncompressedSizes()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert - uncompressed means cbData == cbUncomp
        foreach (var block in cab.Folders[0].DataBlocks)
        {
            Assert.AreEqual(block.cbData, block.cbUncomp);
        }
    }

    [TestMethod]
    public void MSCab_DataBlocks_ContainCorrectData()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Assert - first block should have data
        var firstBlock = cab.Folders[0].DataBlocks[0];
        Assert.IsTrue(firstBlock.ab.Length > 0, "Data block should contain data");
        
        // Verify it's ASCII text
        var content = System.Text.Encoding.ASCII.GetString(firstBlock.ab);
        Assert.IsTrue(content.Length > 0, "Content should not be empty");
        
        // Log for debugging
        Console.WriteLine($"First block data: {content}");
    }

    [TestMethod]
    public void MSCab_Dump_DoesNotThrow()
    {
        // Arrange
        if (!File.Exists(_testCabPath)) Assert.Inconclusive("Test CAB not found");

        using var cab = new MSCab(_testCabPath);

        // Act & Assert - should not throw
        cab.Dump();
    }

    [TestMethod]
    public void CabFlags_None_HasNoFlags()
    {
        // Arrange
        var flags = Cabflags.None;

        // Assert
        Assert.IsFalse(flags.HasFlag(Cabflags.Prev));
        Assert.IsFalse(flags.HasFlag(Cabflags.Next));
        Assert.IsFalse(flags.HasFlag(Cabflags.Reserved));
    }

    [TestMethod]
    public void CabFlags_Prev_HasPrevFlag()
    {
        // Arrange
        var flags = Cabflags.Prev;

        // Assert
        Assert.IsTrue(flags.HasFlag(Cabflags.Prev));
        Assert.IsFalse(flags.HasFlag(Cabflags.Next));
    }

    [TestMethod]
    public void CabFlags_Next_HasNextFlag()
    {
        // Arrange
        var flags = Cabflags.Next;

        // Assert
        Assert.IsTrue(flags.HasFlag(Cabflags.Next));
        Assert.IsFalse(flags.HasFlag(Cabflags.Prev));
    }

    [TestMethod]
    public void CabFlags_Reserved_HasReservedFlag()
    {
        // Arrange
        var flags = Cabflags.Reserved;

        // Assert
        Assert.IsTrue(flags.HasFlag(Cabflags.Reserved));
    }

    [TestMethod]
    public void CabFlags_Combined_HasMultipleFlags()
    {
        // Arrange
        var flags = Cabflags.Prev | Cabflags.Next | Cabflags.Reserved;

        // Assert
        Assert.IsTrue(flags.HasFlag(Cabflags.Prev));
        Assert.IsTrue(flags.HasFlag(Cabflags.Next));
        Assert.IsTrue(flags.HasFlag(Cabflags.Reserved));
    }

    [TestMethod]
    public void CabfileEntry_Constructor_SetsAllProperties()
    {
        // Arrange
        uint fileSize = 12345;
        uint folderStart = 100;
        ushort folder = 2;
        ushort date = 0x1234;
        ushort time = 0x5678;
        FileAttribute attribs = FileAttribute.Archive;
        string filename = "test.txt";

        // Act
        var entry = new CabfileEntry(fileSize, folderStart, folder, date, time, attribs, filename);

        // Assert
        Assert.AreEqual(fileSize, entry.cbFile);
        Assert.AreEqual(folderStart, entry.uoffFolderStart);
        Assert.AreEqual(folder, entry.iFolder);
        Assert.AreEqual(date, entry.date);
        Assert.AreEqual(time, entry.time);
        Assert.AreEqual(attribs, entry.attributes);
        Assert.AreEqual(filename, entry.szName);
    }

    [TestMethod]
    public void CabFolderEntry_Constructor_SetsProperties()
    {
        // Arrange
        uint cabStart = 0x1000;
        ushort cfData = 5;
        int compType = 3;
        int window = 21;

        // Act
        var folder = new CabFolderEntry(cabStart, cfData, compType, window);

        // Assert
        Assert.AreEqual(cabStart, folder.coffCabStart);
        Assert.AreEqual(cfData, folder.cCFData);
        Assert.AreEqual(compType, folder.typeCompress);
        Assert.AreEqual(window, folder.lzxWindow);
        Assert.IsNotNull(folder.DataBlocks);
        Assert.AreEqual(0, folder.DataBlocks.Count);
    }

    [TestMethod]
    public void CabFolderEntry_Length_CalculatesCorrectly()
    {
        // Arrange
        var folder = new CabFolderEntry(0x100, 3, 2, 20);

        // Act
        var length = folder.Length;

        // Assert
        // sizeof(uint) + sizeof(ushort) + sizeof(ushort) + abReserve.Length
        // 4 + 2 + 2 + 0 = 8
        Assert.AreEqual((uint)8, length);
    }

    [TestMethod]
    public void CabFolderEntry_AddBlock_AddsToDataBlocks()
    {
        // Arrange
        var folder = new CabFolderEntry(0x100, 3, 2, 20);
        var block = new CabDataBlock(0, 1024, 2048, [], new byte[1024]);

        // Act
        folder.AddBlock(block);

        // Assert
        Assert.AreEqual(1, folder.DataBlocks.Count);
        Assert.AreEqual(block.csum, folder.DataBlocks[0].csum);
        Assert.AreEqual(block.cbData, folder.DataBlocks[0].cbData);
        Assert.AreEqual(block.cbUncomp, folder.DataBlocks[0].cbUncomp);
    }

    [TestMethod]
    public void CabDataBlock_Constructor_SetsAllProperties()
    {
        // Arrange
        uint checksum = 0xDEADBEEF;
        ushort cbData = 1024;
        ushort cbUncomp = 4096;
        byte[] reserve = new byte[] { 1, 2, 3 };
        byte[] abData = new byte[] { 0x00, 0x01, 0x02 };

        // Act
        var block = new CabDataBlock(checksum, cbData, cbUncomp, reserve, abData);

        // Assert
        Assert.AreEqual(checksum, block.csum);
        Assert.AreEqual(cbData, block.cbData);
        Assert.AreEqual(cbUncomp, block.cbUncomp);
        Assert.AreSame(reserve, block.abReserve);
        Assert.AreSame(abData, block.ab);
    }

    [TestMethod]
    public void CabDataBlock_DefaultConstructor_ReturnsZeroedValues()
    {
        // Arrange & Act
        var block = new CabDataBlock();

        // Assert - default struct constructor leaves fields uninitialized (zero)
        Assert.AreEqual((uint)0, block.csum);
        Assert.AreEqual((ushort)0, block.cbData);
        Assert.AreEqual((ushort)0, block.cbUncomp);
        Assert.IsNull(block.abReserve);
        Assert.IsNull(block.ab);
    }

    [TestMethod]
    public void CabFileEntry_ZeroValues_AreValid()
    {
        // Arrange & Act
        var entry = new CabfileEntry(0, 0, 0, 0, 0, 0, "");

        // Assert
        Assert.AreEqual((uint)0, entry.cbFile);
        Assert.AreEqual((uint)0, entry.uoffFolderStart);
        Assert.AreEqual((ushort)0, entry.iFolder);
        Assert.AreEqual((ushort)0, entry.date);
        Assert.AreEqual((ushort)0, entry.time);
    }

    [TestMethod]
    public void CabFolderEntry_MultipleBlocks_TracksCount()
    {
        // Arrange
        var folder = new CabFolderEntry(0x100, 5, 3, 21);

        // Act
        folder.AddBlock(new CabDataBlock(1, 100, 200, [], new byte[100]));
        folder.AddBlock(new CabDataBlock(2, 100, 200, [], new byte[100]));
        folder.AddBlock(new CabDataBlock(3, 100, 200, [], new byte[100]));

        // Assert
        Assert.AreEqual(3, folder.DataBlocks.Count);
    }
}