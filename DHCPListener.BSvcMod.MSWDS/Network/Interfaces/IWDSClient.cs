using Netboot.Module.DHCPListener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DHCPListener.BSvcMod.MSWDS
{
	internal interface IWDSClient : IDHCPClient, IDisposable
	{
		/// <summary>
		/// Time in seconds between each Try (Default: 4)
		/// </summary>
		public ushort PollInterval { get; set; }

		/// <summary>
		/// How often should the client try to contact the server?  (Default: 5)
		/// </summary>
		public ushort RetryCount { get; set; }

		public PXEPromptOptionValues PXEPromptDone { get; set; }

		public PXEPromptOptionValues PXEPromptAction { get; set; }

		public uint ServerFeatures { get; set; }

		public bool ActionDone { get; set; }
		public NextActionOptionValues NextAction { get; set; }

		public string Message { get; set; }

		public bool ServerSelection { get; set; }

		public uint RequestId { get; set; }

		public bool VersionQuery { get; set; }

		public NBPVersionValues ServerVersion { get; set; }

		public IPAddress ReferralServer { get; set; }

		public NBPVersionValues NBPVersion { get; set; }

		/// <summary>
		/// Internal WDS ID of the Client...
		/// </summary>
		public Guid Id { get; set; }

	}
}
