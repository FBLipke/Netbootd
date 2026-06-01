using System;
using System.IO;

namespace Netboot.Common.Compression.LZX
{
    public class BitBuffer
    {
        public Stream stream;
        public uint bit_buffer;
        public int bits_left;
        byte[] byte_buffer = new byte[ushort.MaxValue];
        int buffer_pos;
        int buffer_end;

        public BitBuffer(Stream s)
        {
            stream = s;
            bit_buffer = 0;
            bits_left = 0;
            buffer_pos = 0;
            buffer_end = 0;
        }

        public void InitBitStream()
        {
            bit_buffer = 0;
            bits_left = 0;

            RefillBuffer();
        }

        bool RefillBuffer()
        {
            if (buffer_pos >= buffer_end)
            {
                buffer_end = stream.Read(byte_buffer, 0, byte_buffer.Length);
                buffer_pos = 0;
                
                if (buffer_end <= 0)
                    return false;
            }

            while (bits_left <= 24 && buffer_pos < buffer_end)
            {
                bit_buffer |= (uint)(byte_buffer[buffer_pos++] << bits_left);
                bits_left += 8;
            }

            return true;
        }

        public uint ReadBits(int count)
        {
            while (bits_left < count)
                if (!RefillBuffer())
                    break;

            var result = bit_buffer & ((1u << count) - 1);
            bit_buffer >>= count;
            bits_left -= count;

            return result;
        }

        public void EnsureBits(int count)
        {
            while (bits_left < count)
                RefillBuffer();
        }

        public uint GetBitsLeft()
            => (uint)((buffer_end - buffer_pos) * 8 + bits_left);

        public int ReadByte()
        {
            if (buffer_pos >= buffer_end)
            {
                buffer_end = stream.Read(byte_buffer, 0, byte_buffer.Length);
                buffer_pos = 0;

                if (buffer_end <= 0)
                    return -1;
            }

            return byte_buffer[buffer_pos++];
        }
    }
}