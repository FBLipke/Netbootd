using Netboot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Utility
{
	internal class NT5DistShare
	{
		public string RootPath = Directory.GetCurrentDirectory();

		public NT5DistShare() {
			Console.WriteLine("[I] This command copies Windows NT CDs into the deployment Share!");
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
			Console.WriteLine(SourcePath);
			// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
			switch (srcType)
			{
				case "nt5":
					var ini = new INIFile(Path.Combine(SourcePath, "I386", "txtsetup.sif".ToUpperInvariant()));
					ini.Open();
					break;
				default:
					break;
			}


		}
	}
}
