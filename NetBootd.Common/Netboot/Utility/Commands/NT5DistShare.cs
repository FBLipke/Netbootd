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

using Netboot.Common;

namespace Netboot.Utility
{
	public class NT5DistShare : IDisposable
	{
		public string RootPath = Directory.GetCurrentDirectory();

		public NT5DistShare()
		{
			Console.WriteLine("[I] Copies Windows NT CDs into the deployment Share!");
		}

		public void Dispose()
		{
		}

		public void Initialize(string[] args)
		{
			RootPath = Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot", "Setup", "German", "WIN2k");
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
				case "ris":

					break;
				default:
					break;
			}
		}
	}
}
