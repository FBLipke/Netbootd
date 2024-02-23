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

using System.Net;

namespace Netboot.Network.Definitions
{
	public class BootServer
	{
		public List<IPAddress> Addresses { get; private set; }

		public string Hostname { get; private set; }

		public BootServerTypes Type { get; set; }

		public BootServer(string hostname, BootServerTypes bootServerType = BootServerTypes.MicrosoftWindowsNT)
		{
			Type = bootServerType;
			Hostname = hostname;

			Addresses = Functions.DNSLookup(Environment.MachineName)
				.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
		}

		public BootServer(IPAddress addr, BootServerTypes bootServerType = BootServerTypes.PXEBootstrapServer)
		{
			Hostname = addr.ToString();
			Type = bootServerType;

			Addresses =
			[
				addr
			];
		}
	}
}
