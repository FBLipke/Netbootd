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

using Netboot.Common.Common;
using Netboot.Common.Utility.Commands;

namespace Netboot.Common.Utility
{
	public class Utility : IDisposable
	{
		public Utility(string[] args)
		{
			Initialize(args);
		}

		public void Initialize(string[] args)
		{
			Console.WriteLine("Netboot utility 0.1a ({0})", Functions.IsLittleEndian()
				? "LE (LittleEndian)" : "BE (BigEndian)");
		}

		public void RunCommand(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Available Commands:");

				return;
			}

			switch (args.First())
			{
				case "!dist":
					Console.WriteLine("!dist: Distribution share management!");
					Console.WriteLine();
					Console.WriteLine("Syntax: !dist (mode) (type) (Disk ROOT)");
					Console.WriteLine("Mode: add/del/mod");
					Console.WriteLine("Type: \"ris\" Performs RIS Operations");
					Console.WriteLine("Type: \"wds\" Performs WDS Operations");

					switch (args[1])
					{
						case "add":
							if (args.Length == 2)
								return;

							switch (args[2])
							{
								case "ris":
									// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
									if (args.Length == 3)
										return;

									using (var nt5dist = new NT5DistShare())
									{
										nt5dist.Initialize(string.Empty, args[2], args[3]);
										nt5dist.Start(args[2], args[3]);
									}
									break;
								case "osx":
									using (var osxdist = new OSXDistShare())
									{
										osxdist.Start(args[2], args[3]);
									}
									break;
								default:
									break;
							}
							break;
						default:
							break;
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

		public void Dispose()
		{
		}
	}
}
