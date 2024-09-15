using System.Xml.Linq;

namespace NetBoot.Common.Netboot.Parser
{
	public class PListFile : Dictionary<string, dynamic>
	{
		public PListFile()
		{
		}

		public PListFile(string file)
			=> Load(file);

		public void Load(string file)
		{
			Clear();

			var dictElements = XDocument.Load(file)
				.Element("plist").Element("dict").Elements();

			Parse(this, dictElements);
		}

		private void Parse(PListFile dict, IEnumerable<XElement> elements)
		{
			for (var i = 0; i < elements.Count(); i += 2)
			{
				var key = elements.ElementAt(i);
				var val = elements.ElementAt(i + 1);

				dict[key.Value] = ParseValue(val);
			}
		}

		private List<dynamic> ParseArray(IEnumerable<XElement> elements)
		{
			List<dynamic> list = [];
			foreach (var e in elements)
				list.Add(ParseValue(e));

			return list;
		}

		private dynamic ParseValue(XElement val)
		{
			switch (val.Name.ToString())
			{
				case "string":
					return val.Value;
				case "integer":
					return int.Parse(val.Value);
				case "real":
					return float.Parse(val.Value);
				case "true":
					return true;
				case "false":
					return false;
				case "dict":
					var plist = new PListFile();
					Parse(plist, val.Elements());
					return plist;
				case "array":
					return ParseArray(val.Elements());
				default:
					throw new ArgumentException("Unsupported");
			}
		}
	}
}
