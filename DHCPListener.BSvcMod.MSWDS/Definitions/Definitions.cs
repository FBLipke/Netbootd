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

namespace DHCPListener.BSvcMod.MSWDS
{
    /// <summary>
    /// Options used by WDSNBP
    /// </summary>
    public enum WDSNBPOptions : byte
    {
        Unknown = 0,
        /// <summary>
        /// Client Arch (LE)
        /// </summary>
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
    /// Options used by NextAction
    /// </summary>
    public enum NextActionOptionValues : byte
    {
        /// <summary>
        /// Administrative: Drop the client request. Administrator has denied the request!
        /// </summary>
        Drop = 0,
        /// <summary>
        /// Require Administrative approval (default)
        /// </summary>
        Approval = 1,
        /// <summary>
        /// Redirect to another server
        /// </summary>
        Referral = 3,
        /// <summary>
        /// Abort PXE boot
        /// </summary>
        Abort = 5
    }

    /// <summary>
    /// Options used by the PXEClientPrompt and PXEPromptDone
    /// </summary>
    public enum PXEPromptOptionValues : byte
    {
        Unknown = 0,
        OptIn,
        NoPrompt,
        OptOut
    }

    /// <summary>
    /// Options used by NBPVersion
    /// </summary>
    public enum NBPVersionValues : ushort
    {
        Seven = 7, // NT5 (Mixed ????)
        Eight = 8, // NT6 (Only WDS ????)
        Unknown = ushort.MinValue
    }
}
