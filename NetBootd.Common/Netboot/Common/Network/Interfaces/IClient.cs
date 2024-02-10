using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBoot.Common.Netboot.Common.Network.Interfaces
{
	public interface IClient : IDisposable
	{
		void Close();
	}
}
