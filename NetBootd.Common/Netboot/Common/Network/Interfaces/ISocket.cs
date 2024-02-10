using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common.Network.Interfaces
{
	public interface ISocket : IDisposable
	{
		void Start();
		void SendTo(byte[] buffer, IPEndPoint endpoint);
		void Close();
	}
}
