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

namespace Netboot.Services.DHCP
{

	public class UpdateBootfileEventargs
	{
		public string Bootfile { get; private set; }

		public string Client { get; private set; }

		public ushort Layer { get; private set; }

		public UpdateBootfileEventargs(string bootfile, ushort layer, string client) {
			Bootfile = bootfile;
			Layer = layer;
			Client = client;
		}
	}
}