/*
 * RAW DHCP Packet Dumper
 * Dumps unknown vendor DHCP packets to binary files for analysis
 */

using System;
using System.IO;
using System.Text;

namespace Netboot.Module.DHCPListener.Utility
{
    /// <summary>
    /// RAW DHCP Packet Dumper for unknown vendor classes
    /// </summary>
    public static class DHCPDumper
    {
        private static readonly string DumpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dump");

        /// <summary>
        /// Dump raw DHCP packet bytes to a .bin file
        /// </summary>
        /// <param name="rawPacket">Raw DHCP packet bytes</param>
        /// <param name="vendor">Vendor class identifier (e.g. "CISCOCAPWAP")</param>
        /// <param name="msgType">DHCP message type (e.g. "DISCOVER", "REQUEST")</param>
        public static void Dump(byte[] rawPacket, string vendor, string msgType)
        {
            try
            {
                var now = DateTime.Now;
                var dateDir = Path.Combine(DumpPath, now.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(dateDir);

                var filename = $"{now:HHmmss}_{msgType}_{SanitizeVendor(vendor)}.bin";
                var path = Path.Combine(dateDir, filename);

                File.WriteAllBytes(path, rawPacket);
            }
            catch
            {
                // Silently fail - dumping is non-critical
            }
        }

        private static string SanitizeVendor(string vendor)
        {
            if (string.IsNullOrEmpty(vendor))
                return "Unknown";

            var sb = new StringBuilder();
            foreach (var c in vendor)
            {
                if (char.IsLetterOrDigit(c) || c == ':' || c == '-' || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            return sb.ToString();
        }
    }
}
