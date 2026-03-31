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

using Netboot.Common.Common.Definitions;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Netboot.Module.DHCPListener
{
    public class BootServer
    {
        public List<IPAddress> Addresses { get; private set; }

        public string Hostname { get; private set; }

        public BootServerType Type { get; private set; }

        public BootServer(string hostname, BootServerType bootServerType)
        {
            Type = bootServerType;
            Hostname = hostname;

            Addresses = Common.Functions.DNSLookup(Hostname)
                .AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();
        }

        public BootServer(IPAddress ipAddr, BootServerType bootServerType)
        {
            Type = bootServerType;
            Addresses = [ipAddr];
            Hostname = Addresses.FirstOrDefault().ToString();
        }

        public byte[] AsBytes(EndianessBehavier endianess = EndianessBehavier.LittleEndian)
        {
            var addressCount = Addresses.Count;
            if (addressCount == 0)
                return [0];

            var addrIndex = 0;
            var ipLength = Addresses.First().GetAddressBytes().Length;
            var addressbuffer = new byte[sizeof(ushort) + sizeof(byte) + (ipLength * addressCount)];

            #region "Bootserver Type (Option)"
            var typeBytes = new byte[sizeof(ushort)];
            switch (endianess)
            {
                case EndianessBehavier.BigEndian:
                    BinaryPrimitives.WriteUInt16BigEndian(typeBytes, (ushort)Type);
                    break;
                case EndianessBehavier.LittleEndian:
                default:
                    BinaryPrimitives.WriteUInt16LittleEndian(typeBytes, (ushort)Type);
                    break;
            }

            Array.Copy(typeBytes, 0, addressbuffer, addrIndex, typeBytes.Length);
            addrIndex += typeBytes.Length;

            #endregion

            #region "IPAddress count (Length)"
            addressbuffer[addrIndex] = Convert.ToByte(addressCount);
            addrIndex += sizeof(byte);
            #endregion

            #region "Addresses"
            foreach (var address in Addresses)
            {
                var addrBytes = address.GetAddressBytes();
                Array.Copy(addrBytes, 0, addressbuffer, addrIndex, addrBytes.Length);
                addrIndex += addrBytes.Length;
            }
            #endregion

            return addressbuffer;
        }
    }
}
