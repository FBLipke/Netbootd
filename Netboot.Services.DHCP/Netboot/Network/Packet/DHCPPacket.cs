using Netboot.Network.Definitions;
using Netboot.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Network.Packet
{
    public class DHCPPacket : BasePacket
    {
        public DHCPPacket(ServerType serverType, byte[] data)
            : base(serverType, data)
        {
        }
    }
}
