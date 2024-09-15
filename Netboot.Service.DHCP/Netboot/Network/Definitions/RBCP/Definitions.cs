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

namespace Netboot.Network.Definitions
{
	public enum BootServerTypes : ushort
	{
		PXEBootstrapServer = 0,
		MicrosoftWindowsNT = 1,
		IntelLCM = 2,
		DOSUNDI = 3,
		NECESMPRO = 4,
		IBMWSoD = 5,
		IBMLCCM = 6,
		CAUnicenterTNG = 7,
		HPOpenView = 8,
		Reserved = 9,
		Vendor = 32768,
		Apple = ushort.MaxValue - 4,
		Linux = ushort.MaxValue - 3,
		BISConfig = ushort.MaxValue - 2,
		WindowsDeploymentServer = ushort.MaxValue - 1,
		ApiTest = ushort.MaxValue
	}
}
