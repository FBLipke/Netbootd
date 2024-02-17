using Netboot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Utility
{
	public class Utility : IDisposable
	{
		public Utility(string[] args) { 
		
		}

		public void Initialize()

		{
			Console.WriteLine("Netboot utility 0.1a ({0})", Functions.IsLittleEndian()
				? "LE (LittleEndian)" : "BE (BigEndian)");
		}

		public void RunCommand(string[] args)
		{
			if (args.Length == 0)
				return;

			switch (args.First())
			{
				case "!dist":
					Console.WriteLine("Mange distribution share!");
					switch (args[1])
					{
						case "add":
							Console.WriteLine("!dist add ->");
							if (args.Length == 2)
								return;

							switch (args[2])
							{
								case "nt5":
									// https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
									Console.WriteLine("Sysntax: !dist add nt5 (CD ROOT)");
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
