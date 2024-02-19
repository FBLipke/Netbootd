namespace Netboot.Common
{
	public class NT5DiskInfo
	{
		public string Name { get; set; } = "Windows NT 5.x Disk";
		public string Tag { get; set; } = "nt5";

		public Dictionary<string, Dictionary<string, string>> SourceDiskFiles { get; set; } = [];

		public NT5DiskInfo() {
		}
	}
}
