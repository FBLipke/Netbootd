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
							Sections.Add(sectionName, new Dictionary<string, string>());
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
			}
		}

		public void Dump()
		{
			foreach (var section in Sections)
			{
				Console.WriteLine($"[{section.Key}]");

				foreach (var value in section.Value)
				{
					Console.WriteLine($"{value.Key} = {value.Value}");
				}

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
				Sections.Add(section, new Dictionary<string, string>());

			if (!Sections[section].ContainsKey(key))
				Sections[section].Add(key,value);
			else
				Sections[section][key] = value;
		}

		public List<string> GetSectionKeys(string section)
			=> Sections[section].Keys.ToList();
	}
}
