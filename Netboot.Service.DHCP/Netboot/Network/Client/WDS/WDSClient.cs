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

using Netboot.Network.Definitions;
using System.Net;

namespace Netboot.Network.Client
{
	public class WDSClient
	{
		public ushort PollInterval { get; set; }

		public ushort RetryCount { get; set; }

		public PXEPromptOptionValues PXEPromptDone { get; set; } = PXEPromptOptionValues.OptIn;

		public PXEPromptOptionValues PXEPromptAction { get; set; } = PXEPromptOptionValues.OptIn;

		public uint ServerFeatures { get; set; }

		public bool ActionDone { get; set; } = false;

		public NextActionOptionValues NextAction { get; set; }
		
		public string AdminMessage { get; set; } = "Waiting for Approval...";

        public bool ServerSelection { get; set; } = true;

		public uint RequestId { get; set; } = 1;
		
		public bool VersionQuery { get; set; }
		
		public NBPVersionValues ServerVersion { get; set; }
		
		public IPAddress ReferralServer { get; set; }
		
		public PXEPromptOptionValues ClientPrompt { get; set; } = PXEPromptOptionValues.OptOut;
		
		public PXEPromptOptionValues PromptDone { get; set; } = PXEPromptOptionValues.OptOut;
		
		public NBPVersionValues NBPVersion { get; set; }

		public WDSClient()
		{
			PollInterval = Convert.ToUInt16(5);
			RetryCount = ushort.MaxValue;
			ActionDone = false;
			AdminMessage = "Waiting for Approval...";
			NextAction = NextActionOptionValues.Approval;
		}
	}
}
