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
	/// <summary>
	/// Options used by the Windows Deployment Server NBP
	/// </summary>
	public enum WDSNBPOptions : byte
	{
		Unknown = 0,
		Architecture = 1,
		NextAction = 2,
		PollInterval = 3,
		PollRetryCount = 4,
		RequestID = 5,
		Message = 6,
		VersionQuery = 7,
		ServerVersion = 8,
		ReferralServer = 9,
		PXEClientPrompt = 11,
		PxePromptDone = 12,
		NBPVersion = 13,
		ActionDone = 14,
		AllowServerSelection = 15,
		ServerFeatures = 16,

		End = byte.MaxValue
	}

	/// <summary>
	/// Options used by the WDSNBPOptions.NextAction
	/// </summary>
	public enum NextActionOptionValues : byte
	{
		Drop = 0,
		Approval = 1,
		Referral = 3,
		Abort = 5
	}

	/// <summary>
	/// Options used by the PXEClientPrompt and PXEPromptDone
	/// </summary>
	public enum PXEPromptOptionValues : byte
	{
		Unknown,
		OptIn,
		NoPrompt,
		OptOut
	}

	/// <summary>
	/// Options used by the NBPVersion
	/// </summary>
	public enum NBPVersionValues : ushort
	{
		Seven = 7,
		Eight = 8,
		Unknown = ushort.MinValue
	}
}
