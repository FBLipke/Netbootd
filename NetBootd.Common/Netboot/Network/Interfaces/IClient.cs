using System;
using System.Net;


namespace Netboot.Network.Interfaces
{
	public interface IClient : IDisposable
	{
		public Guid SocketId { get; set; }
		public Guid ServerId { get; set; }
		public string ServiceType { get; set; }
		public string ClientId { get; set; }
		IPEndPoint RemoteEntpoint { get; set; }

		void Close();
	}
}
