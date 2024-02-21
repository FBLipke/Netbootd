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
using System.Text;

namespace Netboot.Common
{
	public static class Extensions
	{
		public static string GetString(this byte[] input)
			=> GetString(input, Encoding.ASCII);

		public static short Get_Int16(this byte[] input)
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input));
		}

		public static string GetString(this byte[] input, Encoding encoding)
			=> encoding.GetString(input);
	}
}
