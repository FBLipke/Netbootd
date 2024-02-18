using Netboot.Common;

namespace Netboot.Utility
{
	internal class NT5DistShare
	{
		public string RootPath = Directory.GetCurrentDirectory();

		public NT5DistShare() {
			Console.WriteLine("[I] Copies Windows NT CDs into the deployment Share!");
		}

		public void Initialize(string[] args)
		{
			RootPath = Path.Combine(Directory.GetCurrentDirectory(),"TFTPRoot","Setup","German", "WIN2k");
		}

		public void RunCommand(string args)
		{

		}

		public void Start(string srcType, string sourcePath)
		{
			var SourcePath = sourcePath;
			// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
			switch (srcType)
			{
				case "nt5":
					var ini = new INIFile(Path.Combine(Directory.GetCurrentDirectory(), "txtsetup.sif".ToUpperInvariant()));
					ini.Open();

					foreach (var key in ini.GetSectionKeys("SourceDisksNames"))
					{
						Console.WriteLine("key: {0}", key);
					}
					break;
				default:
					break;
			}


		}
	}
}
