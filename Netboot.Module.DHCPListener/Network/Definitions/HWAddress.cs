using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Module.DHCPListener
{
	public class HWAddress : object
	{
		public byte[] Address { get; private set; }
		
		public int Length { get; private set; }

		public HWAddress(byte[] bytes)
		{
			Address = bytes;
			Length = Address.Length;
		}

		public Guid ToGuid() => new(string.Format
			("00000000-0000-0000-0000-{0}", ToString("")));

		public string ToString(string delimeter = ":")
			=> string.Join(delimeter, Address.Select(x => x.ToString("X2")));
	}
}
