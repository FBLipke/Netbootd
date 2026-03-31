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

using Netboot.Module.DHCPListener;

namespace DHCPListener.BSvcMod.BSDP
{
    public class BSDPClient : DHCPClient, IBSDPClient
    {
        public BSDPMsgType BSDPMsgType { get; set; }

        public Version BSDPVersion { get; set; } = new Version(1,1);

        public BSDPClient(bool testClient, DHCPPacket request, Guid server, Guid socket, Guid client)
            : base(testClient, server, socket, client, request)
        {
            var encaps = Request.GetEncOptions((byte)43).Values;
            foreach (var option in encaps)
            {
                switch ((BSDPVendorEncOptions)option.Option)
                {
                    case BSDPVendorEncOptions.MessageType:
                        BSDPMsgType = (BSDPMsgType)option.AsByte();
                        break;
                    case BSDPVendorEncOptions.Version:
                        BSDPVersion = new Version(option.Data.First(), option.Data.Last());
                        break;
                    case BSDPVendorEncOptions.ServerIdentifier:

                        break;
                    case BSDPVendorEncOptions.ServerPriority:
                        break;
                    case BSDPVendorEncOptions.ReplyPort:
                        break;
                    case BSDPVendorEncOptions.BootImageListPath:
                        break;
                    case BSDPVendorEncOptions.DefaultBootImage:
                        break;
                    case BSDPVendorEncOptions.SelectedBootImage:
                        break;
                    case BSDPVendorEncOptions.BootImageList:
                        break;
                    case BSDPVendorEncOptions.Netboot10Firmware:
                        break;
                    case BSDPVendorEncOptions.AttributesFilterList:
                        break;
                    case BSDPVendorEncOptions.MaxMessageSize:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
