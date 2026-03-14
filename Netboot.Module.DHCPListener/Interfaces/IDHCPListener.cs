using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Module.DHCPListener.Interfaces
{
    public interface IDHCPListener
    {
        void Handle_DHCP_Discover(Guid clientid, DHCPPacket request);
        
        void Handle_DHCP_Request(Guid clientid, DHCPPacket request);
    }
}
