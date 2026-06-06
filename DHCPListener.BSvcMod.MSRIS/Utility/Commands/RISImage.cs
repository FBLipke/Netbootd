using Netboot.Common;
using Netboot.Common.Parser;
using System.Xml;

namespace DHCPListener.BSvcMod.MSRIS.Utility.Commands
{
	public class RisImage
	{
		INIFile DOSNET { get; set; }

		INIFile TXTSETUP { get; set; }

		public Version Version { get; private set; }

		public string DestinationPlatform { get; private set; }

		public string InstallPath { get; private set; }

		public string RootDir { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "TFTPRoot");

		public string ConfigDir { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "Config");

		string ImagePath = Directory.GetCurrentDirectory();
		string ImageRoot = Directory.GetCurrentDirectory();

		public string SetupDiskRoot = Directory.GetCurrentDirectory();
		public string NLS = string.Empty;

		Dictionary<string, string> __strings = [];

		public Dictionary<string, string> Directories = [];
		public Dictionary<string, List<string>> FilesToCopy = [];

		public RisImage(string _path)
		{
			if (!Directory.Exists(_path))
			{
				NetbootBase.Log("E", this.GetType().ToString(), string.Format("File or directory not found: {0}", _path));
				return;
			}

			var _filename_dosnet = Directory.GetFiles(_path, "dosnet.inf", SearchOption.AllDirectories).FirstOrDefault();
			if (string.IsNullOrEmpty(_filename_dosnet))
				return;

			var _dosnet = new FileInfo(_filename_dosnet);
			if (!_dosnet.Exists)
			{
				NetbootBase.Log("E", this.GetType().ToString(), string.Format("File or directory not found: {0}", _dosnet.FullName));
				return;
			}

			var _filename_txtsetup = Directory.GetFiles(_path, "txtsetup.sif", SearchOption.AllDirectories).FirstOrDefault();
			if (string.IsNullOrEmpty(_filename_txtsetup))
				return;

			var _txtsetup = new FileInfo(_filename_txtsetup);
			if (!_txtsetup.Exists)
			{
				NetbootBase.Log("E", this.GetType().ToString(), string.Format("File or directory not found: {0}", _txtsetup.FullName));
				return;
			}

			DOSNET = new INIFile(_dosnet.FullName);
			TXTSETUP = new INIFile(_txtsetup.FullName);

			SetupDiskRoot = _path;
		}

		public bool Start()
		{
			#region "Dosnet"
			if (!DOSNET.Open())
				return false;

			var _SrcDirs = DOSNET.GetSectionKeys("Directories");

			var _dosnet_strings = DOSNET.GetSectionKeys("Strings");
			foreach (var __string in _dosnet_strings)
			{
				if (!__strings.ContainsKey(__string))
				{
					var _str = DOSNET.GetValue("Strings", __string).FirstOrDefault();
					if (string.IsNullOrEmpty(_str))
						continue;

					__strings.Add(__string, _str);
				}
			}

			var _str_destPlatform = DOSNET.GetValue("Miscellaneous", "DestinationPlatform", "unknown_arch").FirstOrDefault();
			if (string.IsNullOrEmpty(_str_destPlatform))
				return false;

			DestinationPlatform = _str_destPlatform;
			#endregion

			#region "Txtsetup"
			if (!TXTSETUP.Open())
				return false;

			var _txtsetup_strings = TXTSETUP.GetSectionKeys("Strings");
			foreach (var __string in _txtsetup_strings)
			{
				if (!__strings.ContainsKey(__string))
				{
					var _str = TXTSETUP.GetValue("Strings", __string).FirstOrDefault();
					if (string.IsNullOrEmpty(_str))
						continue;

					__strings.Add(__string, _str);
				}
			}

			#endregion

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

			NetbootBase.Log("I", this.GetType().ToString(), "Copying Files...:");
			Copy(ImagePath);


			CreateAnswerFile(Path.Combine(ImagePath, DestinationPlatform));

			return true;
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

			var xmlFile = Path.Combine(ConfigDir, "ris", "ristndrd.xml");

			if (!File.Exists(xmlFile))
			{
				NetbootBase.Log("E", this.GetType().ToString(), string.Format("File not Found: {0}", xmlFile));
				return;
			}

			{
				var ristndrd_xml = new XmlDocument();
				ristndrd_xml.Load(xmlFile);
				var content = ristndrd_xml.SelectNodes("Netboot/sif/content");

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

					NetbootBase.Log("I", this.GetType().ToString(), string.Format("Creating Answerfile: {0}", tmplFile));

					using (var newIni = new INIFile(tmplFile))
						newIni.SetValues(answerFile);
				}
				#endregion
			}
		}

		bool _copyFile(string src, string dst)
		{
			if (!File.Exists(src))
				return false;

			if (!File.Exists(dst))
				File.Copy(src, dst, true);

			return true;
		}

		void Copy(string destination)
		{
			#region "File Copy"

			foreach (var directory in FilesToCopy)
			{
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
	}
}
