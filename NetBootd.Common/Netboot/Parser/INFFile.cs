using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common
{

	public class INIFile
	{
		Dictionary<string, List<KeyValuePair<string, string>>> Sections 
			= new Dictionary<string, List<KeyValuePair<string,string>>>();
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
							Sections.Add(sectionName, new List<KeyValuePair<string, string>>());
					}
                    else
                    {
						if (!line.Contains('='))
							continue;
						
						var lineParts = line.Split('=');

						var key = lineParts[0].Trim();
						var value = lineParts[1].Trim();


						Sections[sectionName].Add(new KeyValuePair<string, string> { key, value } );













						Console.WriteLine("{0} = {1}", key, value);
                    }
                }


			}
		}

	}
}
