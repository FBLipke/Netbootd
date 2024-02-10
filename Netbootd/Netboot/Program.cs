using Netboot;
using System.Diagnostics;

namespace Netbootd.Netboot
{
	internal class Program
	{
		static NetbootBase? NetbootBase;

		static void Main(string[] args)
		{
			var currentProcess = Process.GetCurrentProcess();
			currentProcess.Exited += CurrentDomain_ProcessExit;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
			AppDomain.CurrentDomain.DomainUnload += CurrentDomain_ProcessExit;

			NetbootBase = new NetbootBase(args);
			if (NetbootBase.Initialize())
			{
				NetbootBase.Start();

				var x = string.Empty;

				while (x != "!exit")
					x = Console.ReadLine();
			}

			CurrentDomain_ProcessExit(null, EventArgs.Empty);
		}

		private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
		{
			NetbootBase?.Stop();
			NetbootBase?.Dispose();
		}
	}
}
