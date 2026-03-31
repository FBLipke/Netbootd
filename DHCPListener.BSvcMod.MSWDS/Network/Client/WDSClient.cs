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

using Netboot.Common.Network.Interfaces;
using Netboot.Module.DHCPListener;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DHCPListener.BSvcMod.MSWDS
{
    public class WDSClient : DHCPClient, IWDSClient
    {
        public PXEPromptOptionValues PXEPromptDone { get; set; }

        public PXEPromptOptionValues PXEPromptAction { get; set; } = PXEPromptOptionValues.OptIn;

        public uint ServerFeatures { get; set; }

        public bool ActionDone { get; set; } = false;

        public NextActionOptionValues NextAction { get; set; } = NextActionOptionValues.Approval;

        public string Message { get; set; } = string.Empty;

        public bool ServerSelection { get; set; } = false;

        public uint RequestId { get; set; } = 0;

        public bool VersionQuery { get; set; } = false;

        public NBPVersionValues ServerVersion { get; set; }

        public IPAddress ReferralServer { get; set; } = IPAddress.Any;

        public NBPVersionValues NBPVersion { get; set; }

        public WDSClient(bool testClient, DHCPPacket request, Guid server, Guid socket, Guid client)
            : base(testClient, server, socket, client, request) { }
    }
}
