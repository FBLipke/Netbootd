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

namespace Netboot.Common
{
	public class INIFile
	{
		Dictionary<string, Dictionary<string, string>> Sections
			= new Dictionary<string, Dictionary<string, string>>();

		string FilePath = string.Empty;

		public INIFile(string filePath)
		{
			FilePath = filePath;
		}

		public void Open()
		{
			using (var reader = new StreamReader(FilePath))
			{
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
						if (!line.Contains('='))
							continue;

						var lineParts = line.Split('=');

						var key = lineParts[0].Trim();
						var value = lineParts[1].Trim();

						if (!Sections[sectionName].ContainsKey(key))
							Sections[sectionName].Add(key, value);
					}
				}

				reader.Close();
			}
		}

		public void Dump()
		{
			foreach (var section in Sections)
			{
				Console.WriteLine($"[{section.Key}]");

				foreach (var value in section.Value)
					Console.WriteLine($"{value.Key} = {value.Value}");

				Console.WriteLine("");
			}
		}

		public bool HasSection(string section)
			=> Sections.ContainsKey(section);

		public string GetValue(string section, string key, string defaultValue = "")
		{
			var value = Sections[section][key];
			return !string.IsNullOrEmpty(value) ? value : defaultValue;
		}

		public void SetValue(string section, string key, string value)
		{
			if (!Sections.ContainsKey(section))
				Sections.Add(section, []);

			if (!Sections[section].TryAdd(key, value))
				Sections[section][key] = value;
		}

        public void SetValues(Dictionary<string, Dictionary<string, string>> data)
        {
			Sections = data;
			Commit();
        }


		public void Commit()
		{
			using (var sw = new StreamWriter(FilePath))
			{
				sw.AutoFlush = true;
				sw.NewLine = "\r\n";

                foreach (var section in Sections)
                {
                    sw.WriteLine($"[{section.Key}]");

                    foreach (var value in section.Value)
                        sw.WriteLine($"{value.Key} = {value.Value}");

                    sw.WriteLine("");
                }

				sw.Close();
            }
        }

        public List<string> GetSectionKeys(string section)
			=> Sections[section].Keys.ToList();
	}
}
