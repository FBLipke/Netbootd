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
using System.Net;
using System.Net.Sockets;

namespace Netboot.Module.BINLListener
{
	public class BINLClient
	{
        public string OSCFileName { get; private set; } = "welcome.osc";

        public string Language { get; private set; } = "englisch";

        public bool NTLMV2Enabled { get; private set; } = false;

        public Guid Socket { get; set; }

        public Guid Server { get; set; }

        public Guid Client { get; set; }

        public BINLPacket Response { get; set; }

        public BINLPacket Request { get; set; }

        public BINLClient(bool testClient, Guid server, Guid socket, Guid client, BINLPacket request)
		{
            Socket = socket;
            Client = client;
            Server = server;
            Request = request;
        }
	}
}
