using System.Diagnostics;

namespace Netboot.Utility
{
	internal class Program
	{
		public static Utility? NetbootUtil;

		static void Main(string[] args)
		{
			var currentProcess = Process.GetCurrentProcess();
			currentProcess.Exited += CurrentDomain_ProcessExit;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
			AppDomain.CurrentDomain.DomainUnload += CurrentDomain_ProcessExit;

			NetbootUtil = new Utility(args);
			NetbootUtil.Initialize();
			
			#region "keep program alive"
			var x = string.Empty;

			while (x != "!exit")
			{
				x = Console.ReadLine();
				if (x != "!exit" && x.Contains(' '))
					NetbootUtil.RunCommand(x.Split(' ', StringSplitOptions.RemoveEmptyEntries));
			}
			#endregion

			CurrentDomain_ProcessExit(null, EventArgs.Empty);
		}

		private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
		{
			NetbootUtil?.Dispose();
		}
	}
}
