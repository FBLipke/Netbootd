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

using Netboot.Network.Client.RBCP;
using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace Netboot.Network.Definitions
{
    public static partial class Functions
    {
        /// <summary>
        /// Generate a Bootserver list...
        /// Item Format: [Type(2)][IPCount(1)][LIST(IPADDR)]
        /// </summary>
        /// <param name="serverlist"></param>
        /// <returns></returns>
        public static DHCPOption GenerateBootServersList(List<BootServer> serverlist)
        {
            var ipBlock = 0;
            #region "How many bytes does we need for the IPAdresses"
            foreach (var server in serverlist)
            {
                var addresses = server.Addresses;
                if (!addresses.Any() || string.IsNullOrEmpty(server.Hostname))
                    continue;

                var ipLength = server.Addresses.First().GetAddressBytes().Length;

                ipBlock += server.Addresses.Count;
            }
            #endregion

            var serverListBlock = new byte[sizeof(byte) + ipBlock * 4 + sizeof(ushort)];
            var offset = 0;

            foreach (var server in serverlist)
            {
                var addresses = server.Addresses;
                if (!addresses.Any() || string.IsNullOrEmpty(server.Hostname))
                    continue;

                #region "Server Type"
                var typeBytes = new byte[sizeof(ushort)];
                BinaryPrimitives.WriteUInt16BigEndian(typeBytes, server.Type);
                Array.Copy(typeBytes, 0, serverListBlock, offset, typeBytes.Length);
                offset += typeBytes.Length;
                #endregion

                #region "IPAddress count"
                serverListBlock[offset] = Convert.ToByte(addresses.Count());
                offset += sizeof(byte);
                #endregion

                #region "Addresses"
                foreach (var addr in addresses)
                {
                    var addrBytes = addr.GetAddressBytes();
                    Array.Copy(addrBytes, 0, serverListBlock, offset, addrBytes.Length);
                    offset += addrBytes.Length;
                }
                #endregion
            }

            return new DHCPOption(8, serverListBlock);
        }

        public static DHCPOption GenerateBootMenue(List<BootServer> servers)
        {
            #region Setup the Menue itself...
            var bootmenue = new List<BootMenueEntry>
            {
                new BootMenueEntry(0, "Local Boot")
            };

            foreach (var server in servers)
            {
                if (!server.Addresses.Any() || string.IsNullOrEmpty(server.Hostname))
                    continue;

                bootmenue.Add(new BootMenueEntry(server.Type, server.Hostname));
            }
            #endregion

            var length = 0;
            foreach (var entry in bootmenue)
                length += entry.Description.Length + sizeof(ushort);

            var menuebuffer = new byte[length + 3];
            var offset = 0;

            foreach (var entry in bootmenue)
            {
                // Type
                var typeBytes = new byte[sizeof(ushort)];
                BinaryPrimitives.WriteUInt16BigEndian(typeBytes, entry.Id);
                Array.Copy(typeBytes, 0, menuebuffer, offset, typeBytes.Length);
                offset += typeBytes.Length;

                // Length of Description
                var descLen = Convert.ToByte(entry.Description.Length);
                menuebuffer[offset] = descLen;
                offset += sizeof(byte);

                // Description
                var descBytes = Encoding.ASCII.GetBytes(entry.Description);
                Array.Copy(descBytes, 0, menuebuffer, offset, descBytes.Length);
                offset += descBytes.Length;
            }

            return new DHCPOption(9, menuebuffer);
        }

        public static List<IPAddress> DNSLookup(string hostname)
            => Dns.GetHostAddresses(hostname).ToList();

        public static DHCPOption GenerateBootMenuePrompt()
        {
            var timeout = Convert.ToByte(byte.MaxValue);
            var prompt = Encoding.ASCII.GetBytes("Select Server...");
            var promptbuffer = new byte[1 + prompt.Length + 1];
            var offset = 0;

            promptbuffer[offset] = timeout;
            offset += sizeof(byte);

            Array.Copy(prompt, 0, promptbuffer, offset, prompt.Length);

            return new DHCPOption(10, promptbuffer);
        }

    }
}
