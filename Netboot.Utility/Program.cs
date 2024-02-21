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
