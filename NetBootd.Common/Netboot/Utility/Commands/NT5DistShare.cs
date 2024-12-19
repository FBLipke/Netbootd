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
using Netboot.Utility.Definitions;
using System;
using System.Net.Http.Headers;
using System.Text;

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

		public string GetCompressedName(string filename)
		{
			var tmp = filename;

			if (!tmp.EndsWith("_"))
			{
				var x = tmp.ToCharArray();
				x[x.Length - 1] = '_';
				tmp = new string(x);
			}

			return tmp;
		}

		public void Start(string srcType, string sourcePath)
		{
			var _root = sourcePath;
			var SourcePath = _root;
			// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
			switch (srcType)
			{
				case "nt5":
					// TODO: Use nt5.yml here...


					var osid = "winnt";
					var SectionsToRead = new List<string>
					{
						"SourceDisksNames"
					};

					if (File.Exists(Path.Combine(SourcePath, "cdrom_ip.5")))
					{
						SectionsToRead.Add("SourceDisksNames.x86");
						osid = "win2000";
					}

					if (File.Exists(Path.Combine(SourcePath, "cdrom_ap.5")))
					{
						SectionsToRead.Add("SourceDisksNames.alpha");
						osid = "win2000";
					}

					if (File.Exists(Path.Combine(SourcePath, "cdrom_mp.5")))
					{
						SectionsToRead.Add("SourceDisksNames.ia64");
						osid = "win2000";
					}

					if (File.Exists(Path.Combine(SourcePath, "cdrom_xp.5")))
					{
						SectionsToRead.Add("SourceDisksNames.axp64");
						osid = "win2000";
					}

					var __path = Path.Combine(SourcePath, "i386", "txtsetup.sif").ToUpperInvariant();
					SourcePath = __path;

					var ini = new INIFile(Path.Combine(SourcePath));
					if (!ini.Open())
					{
						Console.WriteLine("[E] Failed to read open file: {0}", SourcePath);
						return;
					}

					// Try to get the nnative Language....
					var __nlscode = ini.GetValue("nls", "DefaultLayout", string.Empty);
					var __strings = new Dictionary<string, string>
					{
						{ string.Empty, string.Empty }
					};

					foreach (var key in ini.GetSectionKeys("Strings"))
					{
						var val = ini.GetValue("Strings", key, string.Empty).Split(',').FirstOrDefault();
						if (string.IsNullOrEmpty(val))
							continue;

						__strings.Add(key, val);
					}

					var os_sData_arch = ini.GetValue("SetupData", "Architecture");
					var os_sData_Path = ini.GetValue("SetupData", "DefaultPath");

					var os_sData_verMajor = ini.GetValue("SetupData", "MajorVersion");
					var os_sData_verMinor = ini.GetValue("SetupData", "MinorVersion");
					var os_sData_LoadIdent =  __strings[ini.GetValue("SetupData", "LoadIdentifier").Replace("%", string.Empty)];

					var diskID = new Dictionary<string, Dictionary<string, string>>();
					var editionType = "Professional";

					var srcDiscs = new Dictionary<string, Dictionary<string, string>>();

					foreach (var section in SectionsToRead)
					{
						foreach (var key in ini.GetSectionKeys(section))
						{
							#region "Get DiskIDs"
							var k = ini.GetValue(section, key).Split(',');

							if (k[1].StartsWith('\\'))
								k[1] = k[1].Substring(1);

							var idFile = Path.Combine(_root, k[1]);

							if (!File.Exists(idFile))
								continue;

							if (k[1].Contains("_"))
							{
								var _EditionId = k[1].Substring(k[1].IndexOf('_') + 1, ((k[1].IndexOf('.') - 1) - k[1].IndexOf('_')));
							
								if (_EditionId == "ip" || _EditionId == "ap" || _EditionId == "mp" || _EditionId == "xp")
									editionType = "pro";
								if (_EditionId == "is" || _EditionId == "as" || _EditionId == "ms" || _EditionId == "xs")
									editionType = "srv";
							}

							var __key = k[0];

							if (__key.Contains('%'))
								__key = __key.Replace("%", string.Empty);

							if (!diskID.ContainsKey(__key))
							{
								diskID.Add(key, []);

								var __srcPath = k.LastOrDefault();
								if (!__srcPath.StartsWith('\\'))
									__srcPath = k[k.Length - 2];

								diskID[key].Add(__key, __srcPath);
							}
							#endregion
						}
					}

					var TargetDir = Path.Combine(Directory.GetCurrentDirectory(), 
						"TFTPRoot", "Setup", __nlscode.ToUpperInvariant(), string.Concat(osid, ".", editionType));

					if (!Directory.Exists(TargetDir))
						Directory.CreateDirectory(Path.Combine(TargetDir, "templates"));

					#region "Create ristndrd.sif"
					var tmplFile = Path.Combine(TargetDir, "templates", "ristndrd.sif");
					
					if (File.Exists(tmplFile))
						File.Delete(tmplFile);

					var answerFile = new Dictionary<string, Dictionary<string,string>>();

					answerFile.Add("data", []);
					answerFile["data"].Add("floppyless", "\"1\"");
					answerFile["data"].Add("msdosinitiated", "\"1\"");
					answerFile["data"].Add("OriSrc", "\"\\\\%SERVERNAME%\\[#SMBShare#]]\\%INSTALLPATH%\\%MACHINETYPE%\"");
					answerFile["data"].Add("OriTyp", "\"4\"");
					answerFile["data"].Add("LocalSourceOnCd", "1");

					answerFile.Add("SetupData", []);
					answerFile["SetupData"].Add("OsLoadOptions", "\"/noguiboot /fastdetect\"");
					answerFile["SetupData"].Add("SetupSourceDevice", "\"\\Device\\LanmanRedirector\\%SERVERNAME%\\[#SMBShare#]]\\%INSTALLPATH%\"");

					var forceOEMHal = false;
					var forceOEMSCSI = false;

					/**
					 * The following Options are undocumented (experimental)... 
					 **/

					answerFile["SetupData"].Add("ForceOemHal", forceOEMHal ? "1" : "0");
					answerFile["SetupData"].Add("ForceOemScsi", forceOEMSCSI ? "1" : "0");

					answerFile.Add("Unattended", []);
					answerFile["Unattended"].Add("OemPreinstall", "yes");
					answerFile["Unattended"].Add("NoWaitAfterTextMode", "1");
					answerFile["Unattended"].Add("FileSystem", "LeaveAlone");
					answerFile["Unattended"].Add("ExtendOEMPartition", "0");
					answerFile["Unattended"].Add("ConfirmHardware", "no");
					answerFile["Unattended"].Add("NtUpgrade", "no");
					answerFile["Unattended"].Add("Win31Upgrade", "no");
					answerFile["Unattended"].Add("TargetPath", os_sData_Path);
					answerFile["Unattended"].Add("OverwriteOemFilesOnUpgrade", "no");
					answerFile["Unattended"].Add("OemSkipEula", "yes");
					answerFile["Unattended"].Add("InstallFilesPath", "\"\\\\%SERVERNAME%\\[#SMBShare#]]\\%INSTALLPATH%\\%MACHINETYPE%\"");

					answerFile.Add("UserData", []);
					answerFile["UserData"].Add("FullName", "\"%USERFIRSTNAME% %USERLASTNAME%\"");
					answerFile["UserData"].Add("OrgName", "\"%ORGNAME%\"");
					answerFile["UserData"].Add("ComputerName", "%MACHINENAME%");

					answerFile.Add("GuiUnattended", []);
					answerFile["GuiUnattended"].Add("OemSkipWelcome", "1");
					answerFile["GuiUnattended"].Add("OemSkipRegional", "1");
					answerFile["GuiUnattended"].Add("TimeZone", "%TIMEZONE%");
					answerFile["GuiUnattended"].Add("AdminPassword", "\"*\"");

					answerFile.Add("LicenseFilePrintData", []);
					answerFile["LicenseFilePrintData"].Add("AutoMode", "PerSeat");

					answerFile.Add("Display", []);
					answerFile["Display"].Add("ConfigureAtLogon", "1");
					answerFile["Display"].Add("AutoConfirm", "1");
					answerFile["Display"].Add("BitsPerPel", "8");
					answerFile["Display"].Add("XResolution", "800");
					answerFile["Display"].Add("YResolution", "600");
					answerFile["Display"].Add("VRefresh", "60");

					answerFile.Add("Networking", []);
					answerFile["Networking"].Add("ProcessPageSections", "yes");

					answerFile.Add("Identification", []);
					answerFile["Identification"].Add("JoinDomain", "%MACHINEDOMAIN%");
					answerFile["Identification"].Add("CreateComputerAccountInDomain", "yes");
					answerFile["Identification"].Add("DoOldStyleDomainJoin", "yes");

					answerFile.Add("NetProtocols", []);
					answerFile["NetProtocols"].Add("MS_TCPIP", "params.MS_TCPIP");

					answerFile.Add("params.MS_TCPIP", []);
					answerFile["params.MS_TCPIP"].Add("InfID", "MS_TCPIP");
					answerFile["params.MS_TCPIP"].Add("DHCP", "yes");

					answerFile.Add("NetClients", []);
					answerFile["NetClients"].Add("MS_MSClient", "params.MS_MSClient");

					answerFile.Add("params.MS_MSClient", []);
					answerFile["params.MS_MSClient"].Add("InfID", "MS_MSClient");

					answerFile.Add("NetServices", []);
					answerFile["NetServices"].Add("MS_Server", "params.MS_Server");

					answerFile.Add("params.MS_Server", []);
					answerFile["params.MS_Server"].Add("InfID", "MS_Server");
					answerFile["params.MS_Server"].Add("BroadcastsToLanman2Clients", "no");

					answerFile.Add("ServicesSection", []);

					answerFile.Add("RemoteInstall", []);
					answerFile["RemoteInstall"].Add("Repartition", "no");
					answerFile["RemoteInstall"].Add("UseWholeDisk", "no");

					answerFile.Add("OSChooser", []);
					answerFile["OSChooser"].Add("Description", "Windows 2000 Professional");
					answerFile["OSChooser"].Add("Help", "\"[#Description#]\" wird automatisch installiert, ohne dass der Benutzer zur Eingabe aufgefordert wird.");
					
					answerFile["OSChooser"]["Help"] = answerFile["OSChooser"]["Help"].Replace("[#Description#]", answerFile["OSChooser"]["Description"]);
					answerFile["OSChooser"]["Description"] = string.Format("\"{0}\"", answerFile["OSChooser"]["Description"]);
					answerFile["OSChooser"].Add("LaunchFile", "\"%INSTALLPATH%\\%MACHINETYPE%\\templates\\startrom.com\"");
					answerFile["OSChooser"].Add("ImageType", "Flat");
					answerFile["OSChooser"].Add("Version", string.Format("\"{0}.{1}\"", os_sData_verMajor, os_sData_verMinor));

					var newIni = new INIFile(tmplFile);
					newIni.SetValues(answerFile);
					#endregion
					

					var copyFilesSections = new List<string>();
					copyFilesSections.Add("SourceDisksFiles");

					var fileList = new Dictionary<string, NTSrcFileInfo>();

					/** What we get...
					 *  [KEY]        = [VALUE]
					 *  acsmib.dll   = 1,,,,,,,2,0,0
					 *  actsaver.scr = 1,,,,,,,2,0,0,%ChannelScreenSaver%     
					 *  actxprxy.dll = 2,,,,,,,2,0,0     
					 *  adcadmin.chm = 1,,,,,,,21,0,0
					 **/
					foreach (var section in copyFilesSections)
					{
						foreach (var key in ini.GetSectionKeys(section).ToList())
						{
							NTSrcFileInfo fileInfo;

							var s = ini.GetValue(section, key).Split(',');
							
							var val = string.Empty;

							if (s.Length > 10 && s[10].Contains("%"))
								s[10] = s[10].Replace("%", string.Empty);

							if (s.Length == 13)
							{
								val = __strings.ContainsKey(s[10]) ? __strings[s[10]] : string.Empty;
								fileInfo = new(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8], s[9], val, s[11], s[12]);
							}
							else if (s.Length == 12)
							{
								val = __strings.ContainsKey(s[10]) ? __strings[s[10]] : string.Empty;
								fileInfo = new(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8], s[9], val, s[11], string.Empty);
							}
							else if (s.Length == 11)
							{
								val = __strings.ContainsKey(s[10]) ? __strings[s[10]] : string.Empty;
								fileInfo = new(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8], s[9], val, string.Empty, string.Empty);
							}
							else if (s.Length == 10)
							{
								fileInfo = new(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8], s[9], string.Empty, string.Empty, string.Empty);
							}
							else
							{
								fileInfo = new(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8], string.Empty, string.Empty, string.Empty, string.Empty);
							}

							var u = diskID[s[0]].FirstOrDefault().Value;
							var x= Path.Combine(sourcePath.Replace("\\", string.Empty), Path.Combine(u, key));

							fileInfo.Dump();
							fileList.Add(x, fileInfo);

						}
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
