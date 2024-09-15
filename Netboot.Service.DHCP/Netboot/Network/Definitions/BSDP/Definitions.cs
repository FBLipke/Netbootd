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

namespace Netboot.Network.Definitions
{
	public enum BSDPMsgType : byte
	{
		List = 1,
		Select = 2,
		Failed = 3
	}

	public enum BSDPVendorEncOptions
	{
		MessageType = 1,
		Version = 2,
		ServerIdentifier = 3,
		ServerPriority = 4,
		ReplyPOrt = 5,
		/// <summary>
		/// Not Used
		/// </summary>
		BootImageListPath = 6,
		DefaultBootImage = 7,
		SelectedBootImage = 8,
		BootImageList = 9,
		Netboot10Firmware = 10,
		AttributesFilterList = 11,
		MaxMessageSize = 12,
	}
}
