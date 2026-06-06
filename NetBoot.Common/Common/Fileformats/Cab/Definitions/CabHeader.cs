using System.Text;

namespace Netboot.Common.FileFormats.Cab
{
	public struct CabHeader
	{
		public byte[] signature = new byte[4];
		public uint reserved1 = uint.MinValue;
		public uint cbCabinet;
		public uint reserved2 = uint.MinValue;
		public uint coffFiles;
		public uint reserved3 = uint.MinValue;
		public byte versionMinor;
		public byte versionMajor;
		public ushort cFolders;
		public ushort cFiles;
		public Cabflags flags;
		public ushort setID;
		public ushort iCabinet = ushort.MinValue;
		public ushort cbCFHeader = ushort.MinValue;
		public byte cbCFFolder = byte.MinValue;
		public byte cbCFData = byte.MinValue;
		public byte[] abReserve = [];
		public string szCabinetPrev = "";
		public string szDiskPrev = "";
		public string szCabinetNext = "";
		public string szDiskNext = "";

		public CabHeader(ref FileStream stream)
		{
			stream.Read(signature, 0, 4);
			reserved1 = stream.ReadUint32LE();
			cbCabinet = stream.ReadUint32LE();
			reserved2 = stream.ReadUint32LE();
			coffFiles = stream.ReadUint32LE();
			reserved3 = stream.ReadUint32LE();
			versionMinor = (byte)stream.ReadByte();
			versionMajor = (byte)stream.ReadByte();

			cFolders = stream.ReadUint16LE();
			cFiles = stream.ReadUint16LE();
			flags = (Cabflags)stream.ReadUint16LE();
			setID = stream.ReadUint16LE();
			iCabinet = stream.ReadUint16LE();
			cbCFHeader = stream.ReadUint16LE();

			if (flags.HasFlag(Cabflags.Reserved) && cbCFHeader != 0)
				abReserve = new byte[cbCFHeader];

			cbCFFolder = (byte)stream.ReadByte();
			cbCFData = (byte)stream.ReadByte();
			stream.Read(abReserve, 0, abReserve.Length);

			if (flags.HasFlag(Cabflags.Prev))
			{
				szCabinetPrev = stream.ReadString(255, Encoding.Unicode);
				szDiskPrev = stream.ReadString(255, Encoding.Unicode);
			}

			if (flags.HasFlag(Cabflags.Next))
			{
				szCabinetNext = stream.ReadString(255, Encoding.Unicode);
				szDiskNext = stream.ReadString(255, Encoding.Unicode);
			}
		}
	}
}
