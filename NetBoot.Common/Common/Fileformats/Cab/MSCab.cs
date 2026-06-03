using Netboot.Common.Compression;
using Netboot.Common.FileFormats.Cab;
using System.Text;

namespace Netboot.Common.FileFormats
{
    public class MSCab : IDisposable
    {
        public CabHeader Header { get; private set; }

        public List<CabFolderEntry> Folders { get; private set; } = [];
        public List<CabfileEntry> FileEntries { get; private set; } = [];

        uint get_folder_offset()
            => Header.coffFiles - (uint)(Header.cFolders * 8);

        public uint FolderOffset { get => get_folder_offset(); }

        FileStream stream;

        public MSCab(string filename)
        {
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                Header = new CabHeader(ref stream);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }

            #region "Folder"
            stream.Seek(FolderOffset, SeekOrigin.Begin);

            for (var i = 0; i < Header.cFolders; i++)
            {
                var cabStart = stream.ReadUint32LE();
                var cfData = stream.ReadUint16LE();

                ushort compRaw = stream.ReadUint16LE();
                int compType = compRaw & 0x000F;
                int lzxWindow = (compRaw >> 8) & 0x1F;

                Console.WriteLine(
                    $"raw=0x{compRaw:X4}, type={compType}, window={lzxWindow}");

                Folders.Add(new(cabStart, cfData, compType, lzxWindow, Header.cbCFFolder));
            }
            #endregion

            #region "File Entries"
            stream.Seek(Header.coffFiles, SeekOrigin.Begin);

            for (var i = 0; i < Header.cFiles; i++)
            {
                // CFFILE entries are FIXED 64 bytes each per MS-CAB spec.
                // Fields: cbFile(4) + uOffset(4) + iFolder(2) + date(2) + time(2) + attribs(2) = 16 bytes,
                // then szName[64-16] = 48 bytes of name starting at offset 16.
                var buf = new byte[64];
                stream.Read(buf, 0, 64);

                var fileSize = BitConverter.ToUInt32(buf, 0);
                var start = BitConverter.ToUInt32(buf, 4);
                var folder = BitConverter.ToUInt16(buf, 8);
                var date = BitConverter.ToUInt16(buf, 10);
                var time = BitConverter.ToUInt16(buf, 12);
                var attributes = (FileAttribute)BitConverter.ToUInt16(buf, 14);

                // Extract null-terminated filename from szName[64-16] (bytes 16-63)
                var nameLen = 0;
                while (nameLen < 48 && buf[16 + nameLen] != 0) nameLen++;
                var cabFilename = Encoding.ASCII.GetString(buf, 16, nameLen);

                FileEntries.Add(new(fileSize, start, folder, date, time, attributes, cabFilename));
            }
            #endregion

            foreach (var folder in Folders)
            {
                stream.Seek(folder.coffCabStart, SeekOrigin.Begin);

                for (var i = 0; i < folder.cCFData; i++)
                {
                    var csum = stream.ReadUint32LE();
                    var cbData = stream.ReadUint16LE();
                    var uncomp = stream.ReadUint16LE();

                    var reserved = new byte[Header.cbCFData];
                    stream.Read(reserved, 0, reserved.Length);

                    var ab = new byte[cbData];
                    stream.Read(ab, 0, ab.Length);

                    var dBlock = new CabDataBlock(csum, cbData, uncomp, reserved, ab);

                    folder.AddBlock(dBlock);
                }
            }
        }

        byte[] DecompressLZX(byte[] compressed, int expectedSize, int windoworder)
        {
            var decoder = new LzxDecoder(windoworder);
            return decoder.Decode(compressed, expectedSize);
        }

        byte[] DecompressMSZIP(byte[] compressed, int expectedSize)
        {
            var result = new byte[expectedSize];

            // MSZIP kann eine 2-Byte Signatur am Anfang haben (0x4B4A "JK")
            // Wenn vorhanden, überspringen
            var dataStart = 0;
            if (compressed.Length >= 2 && BitConverter.ToUInt16(compressed, 0) == 0x4B4A)
                dataStart = 2;

            using (var deflate = new global::System.IO.Compression.DeflateStream(
                new MemoryStream(compressed, dataStart, compressed.Length - dataStart),
                global::System.IO.Compression.CompressionMode.Decompress))
            {
                var bytesRead = deflate.Read(result, 0, expectedSize);

                if (bytesRead != expectedSize)
                    Array.Resize(ref result, bytesRead);
            }

            return result;
        }

        byte[] Decompress(byte[] compressed, int expectedSize, ushort compressionType, int windoworder)
        {
            switch (compressionType)
            {
                case 0:
                    return compressed;
                case 1:
                case 2:
                    return DecompressMSZIP(compressed, expectedSize);
                case 3:
                    return DecompressLZX(compressed, expectedSize, windoworder);
                default:
                    throw new NotSupportedException($"Unknown compression type: {compressionType}");
            }
        }

        public void Extract()
        {
            var file = FileEntries.First();
            var folder = Folders[file.iFolder];

            using (var memstream = new MemoryStream())
            {
                Console.WriteLine($"CFFILE size = {file.cbFile}");
                foreach (var block in folder.DataBlocks)
                {
                    Console.WriteLine(
                        $"CFDATA: comp={block.cbData} uncomp={block.cbUncomp}");
                }

                foreach (var block in folder.DataBlocks)
                    memstream.Write(Decompress(block.ab, block.cbUncomp, (ushort)folder.typeCompress, folder.lzxWindow));

                memstream.Position = 0;

                using (var fs = File.OpenWrite(file.szName))
                    memstream.CopyTo(fs);
            }
        }

        public void Dump()
        {
            Console.WriteLine("=== MSCAB Dump ===");
            Console.WriteLine("Size of Cabfile: {0} bytes", Header.cbCabinet);
            Console.WriteLine("First CFFILE Entry: {0}", Header.coffFiles);
            Console.WriteLine("Cab Version: {0}",

                new Version(Header.versionMajor,
                   Header.versionMinor));

            Console.WriteLine("Number of Folders in Cab: {0}", Header.cFolders);
            Console.WriteLine("Number of Files in Cab: {0}", Header.cFiles);
            Console.WriteLine("Cab Flags: {0}", Header.flags);
            Console.WriteLine("Cab Set: {0}", Header.setID);

            Console.WriteLine("Number of this Cab in Set: {0}", Header.iCabinet);
            Console.WriteLine("size of per-cabinet reserved area: {0}", Header.cbCFHeader);
            Console.WriteLine("size of per-folder reserved area: {0}", Header.cbCFFolder);
            Console.WriteLine("size of per-data reserved area: {0}", Header.cbCFData);

            Console.WriteLine("Folder-Location: {0}", FolderOffset);
            Console.WriteLine("Folders: {0}", Folders.Count);

            var i = 0;

            foreach (var folder in Folders)
            {
                Console.WriteLine("Blocks in Folder #{0}: {1}", i++, folder.DataBlocks.Count);
                Console.WriteLine("Compression: {0}", folder.typeCompress);
                Console.WriteLine("WindowSize: {0}", folder.lzxWindow);
            }

            Console.WriteLine("Files: {0}", FileEntries.Count);

            foreach (var entry in FileEntries)
                Console.WriteLine("File: {0}", entry.szName);
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
