namespace Netboot.Common.FileFormats.Cab
{
    public struct CabFolderEntry
    {
        public List<CabDataBlock> DataBlocks = [];

        public uint coffCabStart;   /* offset of the first CFDATA block in this folder */
        public ushort cCFData;      /* number of CFDATA blocks in this folder */
        public int typeCompress;    /* compression type indicator */
        public int lzxWindow;
        public byte[] abReserve;    /* (optional) per-folder reserved area */

        public uint Length { get => (uint)(sizeof(uint) + sizeof(ushort) + sizeof(ushort) + abReserve.Length); }

        public CabFolderEntry(uint cabStart, ushort cfData, int compType, int window, byte cfFolder = 0)
        {
            coffCabStart = cabStart;
            cCFData = cfData;
            typeCompress = compType;
            lzxWindow = window;
            abReserve = new byte[cfFolder];
        }

        public void AddBlock(CabDataBlock block)
            => DataBlocks.Add(block);
    };
}
