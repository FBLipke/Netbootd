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

	public enum NetcardType {
		PCI = 2,
		ISA = 3
	}
}
