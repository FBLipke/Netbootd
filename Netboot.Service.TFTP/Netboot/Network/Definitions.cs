using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Service.TFTP.Netboot.Network
{
	public enum TFTPOPCodes
	{
		RRQ = 1,
		WRQ = 2,
		DAT = 3,
		ACK = 4,
		ERR = 5,
		OCK = 6
	}

	public enum TFTPMode
	{
		Octet,
		Mail,
		NetASCII
	}

	public enum TFTPErrorCode
	{
		Unknown,
		FileNotFound,
		AccessViolation,
		DiskFull,
		IllegalOperation,
		UnknownTID,
		FileExists,
		NoSuchUser,
		InvalidOption
	}
}
