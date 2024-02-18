using Netboot.Common;
using System.Security.Cryptography;

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
