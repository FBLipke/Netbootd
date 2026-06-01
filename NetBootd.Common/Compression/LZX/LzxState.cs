namespace Netboot.Common.Compression.LZX
{
    public class LzxState
    {
        public uint window_size;
        public uint actual_size;
        public byte[] window = Array.Empty<byte>();
        public uint window_posn;

        public ushort[] PRETREE_table = Array.Empty<ushort>();
        public byte[] PRETREE_len = Array.Empty<byte>();
        public ushort[] MAINTREE_table = Array.Empty<ushort>();
        public byte[] MAINTREE_len = Array.Empty<byte>();
        public ushort[] LENGTH_table = Array.Empty<ushort>();
        public byte[] LENGTH_len = Array.Empty<byte>();
        public ushort[] ALIGNED_table = Array.Empty<ushort>();
        public byte[] ALIGNED_len = Array.Empty<byte>();

        public uint R0, R1, R2;
        public ushort main_elements;
        public int header_read;
        public uint frames_read;
        public uint block_remaining;
        public uint block_length;
        public LzxConstants.BLOCKTYPE block_type;
        public int intel_curpos;
        public int intel_filesize;
        public int intel_started;
    }
}