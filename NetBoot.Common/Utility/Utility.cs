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

using Netboot.Common.FileFormats;
using Netboot.Common.Utility.Commands;
using System.Reflection.PortableExecutable;

namespace Netboot.Common.Utility
{
	public class Utility : IDisposable
	{
		static NetbootBase? NetbootBase;

		public Utility(string[] args)
		{
			Initialize(args);
		}

		public void Initialize(string[] args)
		{
			NetbootBase = new NetbootBase(args, true);
			NetbootBase.Bootstrap(null);
		}

		public void RunCommand(string[] args)
		{
			if (args.First().StartsWith('!'))
			{
				var module = args.First().Substring(1);

				if (!NetbootBase.UtilProviders.ContainsKey(module))
				{
					if (args.First() == "!mscab")
					{
						if (args.Length == 1)
							return;

						var filename = Path.Combine(NetbootBase.Platform.NetbootDirectory, args[1]);
						if (!File.Exists(filename))
						{
							NetbootBase.Log("E", this.GetType().ToString(), string.Format("File not found: {0}", filename));
							return;
						}

						using (var cabfile = new MSCab(filename))
						{
							cabfile.Dump();
							cabfile.Extract();
						}
						return;
					}
					else
					{
						Usage();
						return;
					}
				}



				if (args.Length == 1)
				{
					Console.WriteLine("Available Commands:");
					Console.WriteLine("add: Add an object");
					Console.WriteLine("remove: remove an object");
					Console.WriteLine("edit: edit an object");
					Console.WriteLine("show: show an object");
					Console.WriteLine("list: list the content for an object");

				}
				else
				{
					switch (args[1])
					{
						case "add":
							NetbootBase.UtilProviders[module].Add(args.SubArray(2, (args.Length - 2)));
							break;
						case "remove":
							NetbootBase.UtilProviders[module].Remove(args.SubArray(2, (args.Length - 2)));
							break;
						case "edit":
							NetbootBase.UtilProviders[module].Modify(args.SubArray(2, (args.Length - 2)));
							break;
						case "list":
							NetbootBase.UtilProviders[module].List(args.SubArray(2, (args.Length - 2)));
							break;
						case "show":
							NetbootBase.UtilProviders[module].Show(args.SubArray(2, (args.Length - 2)));
							break;
						default:
							Usage();
							return;
					}
				}
			}
			else
			{
				Usage();
			}

			switch (args.First())
			{
				case "!list":
					foreach (var module in NetbootBase.UtilProviders)
					{
						Console.WriteLine(module);
					}

					break;

				default:
					break;
				case "!test":
					Console.WriteLine("!test: Netboot tests!!");
					Console.WriteLine();
					Console.WriteLine("Syntax: !test [service]");
					Console.WriteLine("Send test packet to a service!");

					switch (args[2])
					{
						case "dhcpc":

							break;

					}

					break;
			}
		}

		public void Usage()
		{
			Console.WriteLine();
			Console.WriteLine("Syntax: !(module) (mode) (type) (Disk ROOT)");

			foreach (var arg in NetbootBase.UtilProviders)
			{
				Console.WriteLine("!{0}: {1}!", arg.Value.Name, arg.Value.Description);
			}
		}

		public void Dispose()
		{
		}
	}
}
