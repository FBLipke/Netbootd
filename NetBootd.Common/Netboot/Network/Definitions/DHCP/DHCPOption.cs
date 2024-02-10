using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common.Netboot.Network.Definitions.DHCP
{
    public class DHCPOption
    {
        public byte Option { get; private set; }

        public int Length { get; private set; }

        public byte[] Data { get; private set; }

        public DHCPOption(byte opt, int len, byte data) {
            Option = opt;
            Length = len;
            Data = new byte[Length];
        }

        public DHCPOption(byte opt, int len, short data)
        {

        }

        public DHCPOption(byte opt, int len, ushort data)
        {

        }

        public DHCPOption(byte opt, int len, int data)
        {

        }

        public DHCPOption(byte opt, int len, uint data)
        {

        }

        public DHCPOption(byte opt, int len, long data)
        {

        }

        public DHCPOption(byte opt, int len, ulong data)
        {

        }

        public DHCPOption(byte opt, int len, IPAddress data)
        {

        }

        public DHCPOption(byte opt, int len, List<IPAddress> data)
        {

        }

        public DHCPOption(byte opt, string data)
        {

        }
    }
}
