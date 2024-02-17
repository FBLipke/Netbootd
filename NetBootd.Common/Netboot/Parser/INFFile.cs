using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common
{
	
	public class INIFile
	{
		Dictionary<string, List<string>> Sections = new Dictionary<string, List<string>>();
		string FilePath = string.Empty;


		public INIFile(string filePath) {
			FilePath = filePath;
		}

		public void Open()
		{
			using (var reader = new StreamReader(FilePath))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine().Trim();

					while (line.Contains ("  "))
						line = line.Replace("  ", " ");

					if (line.StartsWith('['))
					{
						Console.WriteLine(line);
					}
				}
			}
		}

	}
}
