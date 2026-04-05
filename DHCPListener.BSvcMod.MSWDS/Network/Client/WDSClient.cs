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
using Netboot.Common.Network.Interfaces;
using Netboot.Module.DHCPListener;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DHCPListener.BSvcMod.MSWDS
{
    public class WDSClient : DHCPClient, IWDSClient
    {
        public PXEPromptOptionValues PXEPromptDone { get; set; }

        public PXEPromptOptionValues PXEPromptAction { get; set; } = PXEPromptOptionValues.OptIn;

        public uint ServerFeatures { get; set; }

        public bool ActionDone { get; set; } = false;

        public NextActionOptionValues NextAction { get; set; } = NextActionOptionValues.Approval;

        public string Message { get; set; } = string.Empty;

        public bool ServerSelection { get; set; } = false;

        public uint RequestId { get; set; } = 0;

        public bool VersionQuery { get; set; } = false;

        public NBPVersionValues ServerVersion { get; set; }

        public IPAddress ReferralServer { get; set; } = IPAddress.Any;

        public NBPVersionValues NBPVersion { get; set; }

        public WDSClient(bool testClient, DHCPPacket request, Guid server, Guid socket, Guid client)
            : base(testClient, server, socket, client, request)
        {

        }

        public void Handle_WDS_Options()
        {
            var options = new List<DHCPOption<byte>>
            {
                new((byte)WDSNBPOptions.NextAction, (byte)NextAction),
                new((byte)WDSNBPOptions.PxePromptDone, (byte)PXEPromptDone),
                new((byte)WDSNBPOptions.ActionDone, Convert.ToByte(ActionDone)),
                new((byte)WDSNBPOptions.PollRetryCount, (byte)5),
            };

            var requestIDBytes = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(requestIDBytes, (uint)1);
            options.Add(new((byte)WDSNBPOptions.RequestID, requestIDBytes));

            var polldelayBytes = new byte[sizeof(short)];
            BinaryPrimitives.WriteUInt16BigEndian(polldelayBytes, (byte)5);
            options.Add(new((byte)WDSNBPOptions.PollInterval, polldelayBytes));
            options.Add(new((byte)WDSNBPOptions.PXEClientPrompt, (byte)PXEPromptAction));
            options.Add(new((byte)WDSNBPOptions.AllowServerSelection, ServerSelection));

            switch (NextAction)
            {
                case NextActionOptionValues.Approval:
                    options.Add(new((byte)WDSNBPOptions.Message, Message, Encoding.ASCII));
                    break;
                case NextActionOptionValues.Referral:
                    options.Add(new((byte)WDSNBPOptions.ReferralServer, ReferralServer));
                    break;
                default:
                    break;
            }

            Response.AddOption(new(250, options));
        }
        public void Handle_WDS_Request()
        {
            var wdsData = Request.GetEncOptions(250);
            foreach (var wdsOption in wdsData.Values.ToList())
            {
                switch ((WDSNBPOptions)wdsOption.Option)
                {
                    case WDSNBPOptions.Unknown:
                        break;
                    case WDSNBPOptions.Architecture:
                        Architecture = (Architecture)wdsOption.AsUInt16();
                        break;
                    case WDSNBPOptions.NextAction:
                        NextAction = (NextActionOptionValues)wdsOption.AsByte();
                        break;
                    case WDSNBPOptions.RequestID:
                        RequestId = wdsOption.AsUInt32();
                        break;
                    case WDSNBPOptions.VersionQuery:
                        VersionQuery = true;
                        break;
                    case WDSNBPOptions.ServerVersion:
                        ServerVersion = (NBPVersionValues)wdsOption.AsUInt32();
                        break;
                    case WDSNBPOptions.ReferralServer:
                        ReferralServer = wdsOption.AsIPAddress();
                        break;
                    case WDSNBPOptions.PxePromptDone:
                        PXEPromptDone = (PXEPromptOptionValues)wdsOption.AsByte();
                        break;
                    case WDSNBPOptions.NBPVersion:
                        NBPVersion = (NBPVersionValues)wdsOption.AsUInt16();
                        break;
                    case WDSNBPOptions.ServerFeatures:
                        ServerFeatures = wdsOption.AsUInt32();
                        break;
                    case WDSNBPOptions.ActionDone:
                        ActionDone = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
