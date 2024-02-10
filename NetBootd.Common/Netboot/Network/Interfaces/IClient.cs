using System;
using System.Net;


namespace Netboot.Network.Interfaces
{
    public interface IClient : IDisposable
    {
        IPEndPoint RemoteEntpoint { get; set; }

        void Close();
    }
}
