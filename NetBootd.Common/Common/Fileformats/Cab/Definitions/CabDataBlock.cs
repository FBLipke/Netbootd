namespace Netboot.Common.FileFormats.Cab
{
    public struct CabDataBlock
    {
        public uint csum;    /* checksum of this CFDATA entry */
        public ushort cbData;  /* number of compressed bytes in this block */
        public ushort cbUncomp;    /* number of uncompressed bytes in this block */
        public byte[] abReserve; /* (optional) per-datablock reserved area */
        public byte[] ab;  /* compressed data bytes (cbdata)*/

        public CabDataBlock(uint checksum, ushort sizecom, ushort sizeuncom, byte[] resere, byte[] abData)
        {
            csum = checksum;
            cbData = sizecom;
            cbUncomp = sizeuncom;
            ab = abData;
            abReserve = resere;
        }
    };
}
