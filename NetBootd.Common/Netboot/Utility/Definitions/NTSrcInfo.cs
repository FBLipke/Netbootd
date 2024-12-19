using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Utility.Definitions
{
	public class NTSrcFileInfo
	{
		public string DiskID { get; private set; }

		public string SubDirectory { get; private set; }

		public string Unknown_2 { get; private set; }

		public string CheckSum { get; private set; }

		public string UnUsed_3 { get; private set; }
		
		public string Unused_4 { get; private set; }

		public string BootMediaOrder { get; private set; }

		public string DestinationDir { get; private set; }

		public string UpgradeDisposition { get; private set; }

		public string TextModeDisposition { get; private set; }

		public string DestinationFileName { get; private set; }

		public string SrcDirID { get; private set; }
		
		public string DestDirID { get; private set; }

		public string DiskSrcDir { get; private set; }

		public NTSrcFileInfo(string diskId, string subDir, string unk2, string checksum,
			string unused3, string unused4, string bootMediaOrder, string destDir,
			string UpgDispos, string txtmodeDispos, string destFilename, string srcDirId, 
			string destDirId, string dskSrcDir)
		{
			DiskID = diskId;
			SubDirectory = subDir;
			Unknown_2 = unk2;
			CheckSum = checksum;
			UnUsed_3 = unused3;
			Unused_4 = unused4;
			BootMediaOrder = bootMediaOrder;
			DestinationDir = destDir;
			UpgradeDisposition = UpgDispos;
			TextModeDisposition = txtmodeDispos;
			DestinationFileName = destFilename;
			SrcDirID = srcDirId;
			DestDirID = destDirId;
			DiskSrcDir = dskSrcDir;
		}

		public string Dump() => string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
			DiskID, SubDirectory, Unknown_2, CheckSum, UnUsed_3, Unused_4, BootMediaOrder,
				DestinationDir, UpgradeDisposition, TextModeDisposition, DestinationFileName, SrcDirID, DestDirID);
	}
}
