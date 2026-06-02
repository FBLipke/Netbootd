using System.Net;
using System.Text;

namespace Netboot.Common.Network.Sockets.Interfaces
{
    public interface INetbootClient : IDisposable
    {
        Guid Id { get; set; }
        bool Connected { get; set; }
        IPEndPoint RemoteEndpoint { get; set; }
        void HeartBeat();

        IPEndPoint GetEndPoint();

        void Send(ref byte[] data, bool keepalive);
        void Send(ref byte[] data);
        void Send(IPEndPoint remoteEndpoint, ref byte[] data);
        void Send(string data, bool keepAlive);
        void Send(MemoryStream data, bool keepAlive);
        void Send(IPEndPoint remoteEndpoint, ref MemoryStream data);
        void Send(string data, Encoding encoding, bool keepAlive);

        void Close();
        void Start();
        void Disconnect();
        void Read();
    }
}
