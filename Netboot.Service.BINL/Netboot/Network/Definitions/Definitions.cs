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
	public enum BINLMessageTypes : uint
	{
		/// <summary>
		/// NEQ
		/// </summary>
		Negotiate = 0x814e4547,
		/// <summary>
		/// CHL
		/// </summary>
		Challenge = 0x8243484c,
		/// <summary>
		/// AUT
		/// </summary>
		Authenticate = 0x81415554,
		/// <summary>
		/// AU2
		/// </summary>
		AuthenticateFlipped = 0x81415532,
		/// <summary>
		/// RES
		/// </summary>
		Result = 0x82524553,
		/// <summary>
		/// RQU
		/// </summary>
		RequestUnsigned = 0x81525155,
		/// <summary>
		/// RSU
		/// </summary>
		ResponseUnsigned = 0x82525355,
		/// <summary>
		/// REQ
		/// </summary>
		RequestSigned = 0x81524551,
		/// <summary>
		/// RSP
		/// </summary>
		ResponseSigned = 0x82525350,
		/// <summary>
		/// ERR
		/// </summary>
		ErrorSigned = 0x82455252,
		/// <summary>
		/// UNR
		/// </summary>
		UnrecognizedClient = 0x82554e52,
		/// <summary>
		/// OFF
		/// </summary>
		Logoff = 0x814f4646,
		/// <summary>
		/// NAK
		/// </summary>
		NegativeAck = 0x824e414b,
		/// <summary>
		/// NCQ
		/// </summary>
		NetcardRequest = 0x814e4351,
		/// <summary>
		/// NCR
		/// </summary>
		NetcardResponse = 0x824e4352,
		/// <summary>
		/// NCE
		/// </summary>
		NetcardError = 0x824e4345,
		/// <summary>
		/// HLQ
		/// </summary>
		HalRequest = 0x81484c51,
		/// <summary>
		/// HLR
		/// </summary>
		HalResponse = 0x82484c52,
		/// <summary>
		/// SPQ
		/// </summary>
		SetupRequest = 0x81535051,
		/// <summary>
		/// SPS
		/// </summary>
		SetupResponse = 0x82535053
	}

	public enum NetcardRequestVersion
	{
		Version_2 = 2,
	}

	public enum NetcardType
	{
		PCI = 2,
		ISA = 3
	}
}
