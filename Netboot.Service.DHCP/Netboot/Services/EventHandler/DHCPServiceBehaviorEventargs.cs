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

using Netboot.Network.Definitions;

namespace Netboot.Services.DHCP
{
	public class DHCPServiceBehaviorEventargs
	{
		public BootServerTypes BootServerType { get; private set; }

		public byte MenueTimeout { get; private set; }

		public byte RespondDelay { get; private set; }

		public DHCPServiceBehaviorEventargs(BootServerTypes bootServerType, byte timeout, byte respondDelay)
		{
			BootServerType = bootServerType;
			MenueTimeout = timeout;
			RespondDelay = respondDelay;
		}
	}
}