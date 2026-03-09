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

namespace Netboot.Common.Parser
{
	public class INIFile
	{
		Dictionary<string, Dictionary<string, List<string>>> Sections = [];

		string FilePath = string.Empty;
		bool isOpen = false;

		public INIFile(string filePath)
			=> FilePath = filePath;

		public bool Open(bool ReplaceDuplicates = true)
		{
			if (!File.Exists(FilePath))
				return false;

			using (var reader = new StreamReader(FilePath))
			{
				isOpen = true;
				var sectionName = string.Empty;

				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine().Trim();

					if (string.IsNullOrEmpty(line))
						continue;

					while (line.Contains("  "))
						line = line.Replace("  ", " ");

					if (line.StartsWith('['))
					{
						sectionName = line.Substring(1, line.IndexOf(']') - 1);

						if (!Sections.ContainsKey(sectionName))
							Sections.Add(sectionName, []);
					}
					else
					{
						string? value;
						string? key;

						if (line.Contains('='))
						{
							var lineParts = line.Split('=');
							key = lineParts[0].Trim();
							value = lineParts[1].Trim();
						}
						else
						{
							/*
							 *  Windows NT Setup (dosnet.inf) example:
							 *  
							 *	[ServicesToStopInstallation]
							 *	d,winnt32u.dll,UnsupportedArchitectureCheck,,1
							 *	d,winntupg\boscomp.dll,BosHardBlockCheck,,0
							 *	d,winntupg\ntdsupg,DsUpgradeCompatibilityCheck,,0,1
							 *
							 * Solution Read and return as Key ...
							 */

							var parts = line.Split(',', 1);

							key = parts.FirstOrDefault();
							value = parts.LastOrDefault();
						}

						if (!Sections[sectionName].ContainsKey(key))
							Sections[sectionName].Add(key, [value]);
						else
							Sections[sectionName][key].Add(value);
					}
				}

				reader.Close();
				isOpen = false;
			}

			return true;
		}

		public void Dump()
		{
			foreach (var section in Sections)
			{
				Console.WriteLine($"[{section.Key}]");

				foreach (var value in section.Value)
				{
					if (value.Value.Count > 1)
						Console.WriteLine($"{value.Key} = { string.Join(',', value.Value.ToArray())}");
					else
						Console.WriteLine($"{value.Key} = { value.Value.FirstOrDefault()}");
				}

				Console.WriteLine("");
			}
		}

		public bool HasSection(string section)
			=> Sections.ContainsKey(section);

		public IEnumerable<string> GetValue(string section, string key, string defaultValue = "")
			=> Sections[section][key];

		public IEnumerable<string> GetValues(string section, string key, string delimiter = ",")
			=> Sections[section][key];

		public void SetValue(string section, string key, string value)
		{
			if (!Sections.ContainsKey(section))
				Sections.Add(section, []);

			Sections[section][key].Add(value);
		}

		public void SetValues(Dictionary<string, Dictionary<string, List<string>>> data)
		{
			Sections = data;
			Commit();
		}

		public void Commit()
		{
			using (var sw = new StreamWriter(FilePath))
			{
				isOpen = true;

				sw.AutoFlush = true;
				sw.NewLine = "\r\n";

				foreach (var section in Sections)
				{
					sw.WriteLine($"[{section.Key}]");

					foreach (var value in section.Value)
						sw.WriteLine($"{value.Key} = {value.Value.FirstOrDefault()}");

					sw.WriteLine("");
				}

				sw.Close();
				isOpen = false;
			}
		}

		public IEnumerable<string> GetSectionKeys(string section)
			=> Sections[section].Keys.ToList();
	}
}
