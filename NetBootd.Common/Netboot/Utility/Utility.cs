﻿/*
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

namespace Netboot.Utility
{
	public class Utility(string[] args) : IDisposable
	{
		public void Initialize()
		{
			Console.WriteLine("Netboot utility 0.1a ({0})", Functions.IsLittleEndian()
				? "LE (LittleEndian)" : "BE (BigEndian)");
		}

		public void RunCommand(string[] args)
		{
			if (args.Length == 0)
				return;

			Console.WriteLine("!dist: Distribution share management!");

			switch (args.First())
			{
				case "!dist":
					Console.WriteLine("Syntax: !dist add (OStype) (CD ROOT)");
					Console.WriteLine("OSType: \"nt5\" (Windows 2K/XP/2003)");
					switch (args[1])
					{
						case "add":
							if (args.Length == 2)
								return;

							switch (args[2])
							{
								case "nt5":
									// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
									if (args.Length == 3)
										return;

									var nt5dist = new NT5DistShare();
									nt5dist.Start(args[2], args[3]);
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
			}
		}

		public void Dispose()
		{
		}
	}
}