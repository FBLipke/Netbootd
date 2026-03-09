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

using Netboot.Common.Parser;
using System.Xml;
using YamlDotNet.Core.Tokens;

namespace Netboot.Common.Utility.Commands
{
	public class NT5DistShare : IDisposable
	{
		public INIFile DOSNET { get; private set; }
		public INIFile TXTSETUP { get; private set; }

		public Version Version { get; private set; }

		public string DestinationPlatform { get; private set; }

		public string InstallPath {get; private set; }
		
		public string RootDir { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot");
		
		public string ConfigDir { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "Config");

		string ImagePath = Directory.GetCurrentDirectory();
		string ImageRoot = Directory.GetCurrentDirectory();

		public string SetupDiskRoot = Directory.GetCurrentDirectory();
		public string NLS = string.Empty;

		Dictionary<string, string> __strings = [];

		public Dictionary<string, string> Directories = [];
		public Dictionary<string, List<string>> FilesToCopy = [];

		public NT5DistShare()
		{
		}

		public void Dispose()
		{}

		public void Initialize(string configDir, string srcType, string sourcePath)
		{
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Clear();
			Console.WriteLine("[I] Copies Windows NT CDs into the deployment Share!");

		}

		bool GetPathOfDosnet(string _path, string dosnetfile = "dosnet.inf")
		{
			var _dosnet = new FileInfo(Directory.GetFiles(_path, dosnetfile, SearchOption.AllDirectories).FirstOrDefault());
			var _txtsetup = new FileInfo(Directory.GetFiles(_path, "txtsetup.sif", SearchOption.AllDirectories).FirstOrDefault());

			if (!_dosnet.Exists)
			{
				Console.WriteLine("File not found: {0}", _dosnet.FullName);
				return false;
			}

			if (!_txtsetup.Exists)
			{
				Console.WriteLine("File not found: {0}", _txtsetup.FullName);
				return false;
			}


			DOSNET = new INIFile(_dosnet.FullName);
			TXTSETUP = new INIFile(_txtsetup.FullName);

			if (!DOSNET.Open())
				return false;

			if (!TXTSETUP.Open())
				return false;

			SetupDiskRoot = _path;
			var _SrcDirs = DOSNET.GetSectionKeys("Directories");


			var _dosnet_strings = DOSNET.GetSectionKeys("Strings");

			foreach (var __string in _dosnet_strings)
			{
				if (!__strings.ContainsKey(__string))
					__strings.Add(__string, DOSNET.GetValue("Strings", __string).FirstOrDefault());
			}

			var _txtsetup_strings = TXTSETUP.GetSectionKeys("Strings");

			foreach (var __string in _txtsetup_strings)
			{
				if (!__strings.ContainsKey(__string))
					__strings.Add(__string, TXTSETUP.GetValue("Strings", __string).FirstOrDefault());
			}

			DestinationPlatform = DOSNET.GetValue("Miscellaneous", "DestinationPlatform", "unknown_arch").FirstOrDefault();
			RootDir = Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot");
			ConfigDir = Path.Combine(Directory.GetCurrentDirectory(), "Config");

			InstallPath = Path.Combine("Setup", "Images", Guid.NewGuid().ToString());
			ImagePath = Path.Combine(RootDir, InstallPath);

			FilesToCopy = [];

			foreach (var key in _SrcDirs)
			{
				var _folder = DOSNET.GetValue("Directories", key).FirstOrDefault().Trim();

				var _pathEntry = Path.Combine(SetupDiskRoot, _folder.Substring(_folder.LastIndexOf('\\') + 1));

				var _fileListFromDosnet = DOSNET.GetSectionKeys("Files");

				foreach (var entry in _fileListFromDosnet)
				{
					var parts = entry.Split(",");

					if (!FilesToCopy.ContainsKey(parts.First()))
						FilesToCopy.Add(parts.First(), []);

					for (var iP = 1; iP < parts.Length; iP++)
						FilesToCopy[parts.First()].Add(Path.Combine(_pathEntry, parts[iP].Trim()));
				}
			}
			
			
			return true;
		}

		void Copy(string destination)
		{
			#region "File Copy"
			Console.WriteLine("[I] Copyng Files...");
			foreach (var directory in FilesToCopy)
			{
				Console.WriteLine($"{directory.Key}:");

				foreach (var _file in directory.Value)
				{
					var dstPath = Path.Combine(destination, _file.Substring(_file.IndexOf('\\') + 1));
					Directory.CreateDirectory(dstPath.Substring(0, dstPath.LastIndexOf('\\')));

					if (!_copyFile(_file, dstPath))
					{
						var compSrcFileName = _file.Substring(0, _file.Length - 1);
						compSrcFileName += "_";

						var compdstFileName = dstPath.Substring(0, dstPath.Length - 1);
						compdstFileName += "_";

						_copyFile(compSrcFileName, compdstFileName);
					}
				}
			}
			#endregion
		}

		bool _copyFile (string src, string dst)
		{
			try
			{
				if (!File.Exists(src))
					return false;

				File.Copy(src, dst, true);

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		void CreateAnswerFile(string osImageDir)
		{
			#region "Read Informations from txtsetup.sif"

			// OS Version 
			Version = new Version(int.Parse(TXTSETUP.GetValue("SetupData", "MajorVersion").FirstOrDefault()),
				int.Parse(TXTSETUP.GetValue("SetupData", "MinorVersion").FirstOrDefault()));

			var os_sData_LoadIdent = __strings[TXTSETUP.GetValue("SetupData", "LoadIdentifier")
				.FirstOrDefault().Replace("%", string.Empty)];

			var launchFilePath = "%INSTALLPATH%\\%MACHINETYPE%\\templates\\startrom.com"
				.Replace("%MACHINETYPE%", DestinationPlatform).Replace("%INSTALLPATH%", InstallPath);

			#endregion

			var tplDir = Path.Combine(osImageDir, "templates");
			Directory.CreateDirectory(tplDir);

			var ristndrd_xml = new XmlDocument();
				ristndrd_xml.Load(Path.Combine(ConfigDir, "ris", "ristndrd.xml"));
				var content = ristndrd_xml.SelectNodes("Netboot/sif/content");

				Console.WriteLine("[I] Creating File (ristndrd.sif)...");

				var answerFile = new Dictionary<string, Dictionary<string, List<string>>> { };

				foreach (XmlNode xmlnode in content)
				{
					foreach (XmlNode childNode in xmlnode.ChildNodes)
					{
						var section = childNode.Name;
						answerFile.Add(section, []);

						foreach (XmlNode child in childNode.ChildNodes)
							answerFile[section].Add(child.Name, [child.InnerText
								.Replace("%MACHINETYPE%", DestinationPlatform)
									.Replace("%SERVERNAME%", Environment.MachineName)
										.Replace("%INSTALLPATH%", InstallPath)
											.Replace ("[[#LaunchFile#]]", launchFilePath)
												.Replace ("[[#ImageType#]]", "Flat")
													.Replace("[[#Description#]]", os_sData_LoadIdent)
														.Replace("[[#Help#]]", os_sData_LoadIdent)
															.Replace ("[[#Version#]]",
																string.Format("\"{0}.{1}\"", Version.Major, Version.Minor))
						]);
					}
				}

			#region "Create ristndrd.sif"
			{

				var tmplFile = Path.Combine(tplDir, "ristndrd.sif");
				if (File.Exists(tmplFile))
					File.Delete(tmplFile);

				var newIni = new INIFile(tmplFile);
				newIni.SetValues(answerFile);
				newIni.Dump();
			}
			#endregion
		}


		public void Start(string srcType, string sourcePath)
		{
			var SourcePath = sourcePath;
			// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
			switch (srcType)
			{
				case "nt5":
				case "ris":
					var retval = GetPathOfDosnet(sourcePath);

					if (!retval)
						break;
					
					Copy(ImagePath);
					CreateAnswerFile(Path.Combine(ImagePath, DestinationPlatform));

					break;
				default:
					break;
			}

			Console.WriteLine("[I] Done!");
		}
	}
}
