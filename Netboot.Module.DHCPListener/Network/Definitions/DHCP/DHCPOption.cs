/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Netboot.Common.Common.Definitions;
using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace Netboot.Module.DHCPListener
{
    public class DHCPOption<T>
    {
        public byte Option { get; private set; }

        public byte Length { get; private set; }

        public byte[] Data { get; private set; }

        public DHCPOption(T option)
        {
            Option = Convert.ToByte(option);
            Data = [];
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, byte data)
        {
            Option = Convert.ToByte(option);
            Length = 1;
            Data = new byte[Length];
            Data[0] = data;
        }

        public DHCPOption(T option, byte[] data)
        {
            Option = Convert.ToByte(option);
            Length = Convert.ToByte(data.Length);
            Data = data;
        }


        void DHCPOptionFunc<C>(T option, List<DHCPOption<C>> list)
        {
            var length = 0;

            foreach (var item in list)
                length += item.Option != byte.MaxValue ? 2 + item.Length : 1;

            var offset = 0;
            var block = new byte[length];

            foreach (var item in list)
            {
                block[offset] = Convert.ToByte(item.Option);
                offset += sizeof(byte);

                if (item.Option == byte.MaxValue)
                    break;

                if (item.Length == 0)
                    continue;

                block[offset] = item.Length;
                offset += sizeof(byte);

                Array.Copy(item.Data, 0, block, offset, item.Data.Length);
                offset += item.Data.Length;
            }

            Option = Convert.ToByte(option);
            Data = block;
            Length = Convert.ToByte(length);

        }

        public DHCPOption(T option, List<DHCPOption<BSDPVendorEncOptions>> list)
        {
            DHCPOptionFunc(option, list);
        }

        public DHCPOption(T option, List<DHCPOption<byte>> list)
        {
            DHCPOptionFunc(option, list);
        }

        public DHCPOption(T option, List<DHCPOption<DHCPOptions>> list) => DHCPOptionFunc(option, list);

        public DHCPOption(T option, List<DHCPOption<PXEVendorEncOptions>> list) => DHCPOptionFunc(option, list);

        public DHCPOption(T option, short data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, ushort data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, int data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, bool data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, uint data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, long data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, ulong data)
        {
            Option = Convert.ToByte(option);
            Data = BitConverter.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, string data, Encoding encoding)
        {
            Option = Convert.ToByte(option);
            Data = encoding.GetBytes(data);
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, IPAddress data)
        {
            Option = Convert.ToByte(option);
            Data = data.GetAddressBytes();
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, ClientIdentType type, Guid data)
        {
            var bytes = new byte[17];
            bytes[0] = (byte)type;

            var uuidbytes = data.ToByteArray();
            Array.Copy(uuidbytes, 0, bytes, 1, uuidbytes.Length);

            Option = Convert.ToByte(option);
            Data = bytes;
            Length = Convert.ToByte(Data.Length);
        }

        public DHCPOption(T option, Guid data)
        {
            Option = Convert.ToByte(option);
            Data = data.ToByteArray();
            Length = Convert.ToByte(Data.Length);
        }

        public ushort AsUInt16(EndianessBehavier endianess = EndianessBehavier.LittleEndian, int offset = 0)
        {
            var result = new byte[sizeof(ushort)];
            Array.Copy(Data, offset, result, 0, result.Length);

            return endianess switch
            {
                EndianessBehavier.BigEndian => BinaryPrimitives.ReadUInt16BigEndian(result),
                _ => BinaryPrimitives.ReadUInt16LittleEndian(result),
            };
        }

        public byte AsByte()
            => Data.FirstOrDefault();

        public uint AsUInt32(EndianessBehavier endianess = EndianessBehavier.LittleEndian, int offset = 0)
        {
            var result = new byte[sizeof(uint)];
            Array.Copy(Data, offset, result, 0, result.Length);

            return endianess switch
            {
                EndianessBehavier.BigEndian => BinaryPrimitives.ReadUInt32LittleEndian(result),
                _ => BinaryPrimitives.ReadUInt32BigEndian(result),
            };
        }

        public string AsString(Encoding encoding)
            => encoding.GetString(Data);

        public string AsString()
            => AsString(Encoding.ASCII);

        public IPAddress AsIPAddress()
            => new IPAddress(Data);

        public bool AsBool()
            => Data.First() == 1;
    }
}
