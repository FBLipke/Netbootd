namespace Netboot.Common.FileFormats.Cab
{
	public struct CabfileEntry
	{
		public uint cbFile;  /* uncompressed size of this file in bytes */
		public uint uoffFolderStart; /* uncompressed offset of this file in the folder */
		public ushort iFolder; /* index into the CFFOLDER area */
		public ushort date;    /* date stamp for this file */
		public ushort time;    /* time stamp for this file */
		public FileAttribute attributes; /* attribute flags for this file */
		public string szName;  /* name of this file */

		public CabfileEntry(uint fileSize, uint folderStart, ushort folder,
			ushort da, ushort ti, FileAttribute attribs, string filename)
		{
			cbFile = fileSize;
			uoffFolderStart = folderStart;
			iFolder = folder;
			date = da;
			time = ti;
			attributes = attribs;
			szName = filename;
		}
	};
}
