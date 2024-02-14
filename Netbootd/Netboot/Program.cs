using Netboot;
using System.Diagnostics;

namespace Netbootd.Netboot
{
	internal class Program
	{
		static NetbootBase? NetbootBase;
		static bool IsExiting = false;

		public static void HeartBeat () {
			while (!IsExiting)
			{
				Thread.Sleep (10000);
				NetbootBase.Heartbeat();
			}
		}

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

				var heartbeatThread = new Thread(new ThreadStart(HeartBeat));
				heartbeatThread.Start();

				while (x != "!exit")
					x = Console.ReadLine();

				heartbeatThread.Join();
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
