using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPListener.BSvcMod.RBCP.Definitions
{
	public class Definitions
	{
		public enum RBCPLayer
		{
			Bootfile = 0,
			Credential
		}

		public enum PXEOptions : byte
		{
			MultiCastIPAddress = 1,
			MulticastClientPort = 2,
			MulticastServerPort = 3,
			MulticastTFTPTimeout = 4,
			MulticastTFTPDelay = 5,
			DiscoveryControl = 6,
			DiscoveryMulticastAddress = 7,
			BootServer = 8,
			BootMenue = 9,
			MenuPrompt = 10,
			MulticastAddressAllocation = 11,
			CredentialTypes = 12,
			NetworkCardPath = 64,
			ManagementInformation = 65,
			OSInformation = 66,
			BootOSInfo = 67,
			PromptInfo = 68,
			OSInformation2 = 69,
			BootOSInfo2 = 70,
			BootItem = 71,

			#region "LCM Options (see: https://gitlab.com/wireshark/wireshark/-/issues/15498 for more informations...)"

			LCMServer = 179,
			LCMDomain = 180,
			LCMNicOptions = 181,
			LCMWorkGroup = 190,
			LCMDiscovery = 191,
			LCMConfigured = 192,
			LCMVersion = 193,
			#endregion

			End = byte.MaxValue
		}
	}
}
