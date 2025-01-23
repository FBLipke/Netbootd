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

using Netboot;
using System.Diagnostics;

namespace Netbootd.Netboot
{
	internal class Program
	{
		static NetbootBase? NetbootBase;
		static bool IsExiting = false;

		public static void HeartBeat()
		{
			while (!IsExiting)
			{
				Thread.Sleep(10000);
				
				var controlDate = DateTime.Now;

				NetbootBase.Heartbeat(controlDate);
			}
		}

		[STAThread]
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

				#region "keep program alive"
				var x = string.Empty;

				var heartbeatThread = new Thread(new ThreadStart(HeartBeat));
				heartbeatThread.Start();

				while (x != "!exit")
					x = Console.ReadLine();
				#endregion

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
