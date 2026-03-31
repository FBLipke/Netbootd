using Netboot.Module.DHCPListener;
using System.Net;


namespace DHCPListener.BSvcMod.MSWDS
{
    public interface IWDSClient : IDHCPClient, IDisposable
    {
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

        public IPAddress? ReferralServer { get; set; }

        public NBPVersionValues NBPVersion { get; set; }
    }
}
