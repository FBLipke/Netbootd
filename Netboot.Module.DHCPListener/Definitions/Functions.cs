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

namespace Netboot.Module.DHCPListener
{
    public static class Functions
    {
        public static Guid AsLittleEndianGuid(byte[] bytes)
        {
            var idBytes = new byte[16];

            Array.Copy(bytes, idBytes, idBytes.Length);
            Array.Reverse(idBytes, 0, 4);
            Array.Reverse(idBytes, 4, 2);
            Array.Reverse(idBytes, 6, 2);

            return new Guid(idBytes);
        }
    }
}
