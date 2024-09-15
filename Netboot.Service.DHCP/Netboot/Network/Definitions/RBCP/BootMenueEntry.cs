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

namespace Netboot.Network.Client.RBCP
{
	public class BootMenueEntry
	{
		public ushort Id { get; private set; }

		public string Description { get; private set; }

		public byte Length { get; private set; }

		public BootMenueEntry(ushort id, string desc)
		{
			Id = id;
			Description = desc;
			Length = (byte)Description.Length;
		}
	}
}
