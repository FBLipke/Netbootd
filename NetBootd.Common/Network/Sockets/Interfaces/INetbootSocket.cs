using System.Net;
using System.Text;

namespace Netboot.Common.Network.Sockets.Interfaces
{
    public interface INetbootSocket : IDisposable
    {
        Dictionary<Guid, INetbootClient> Clients { get; set; }

        Guid Id { get; set; }

        bool Listening { get; set; }

        IPAddress MulticastGroup { get; set; }

        byte MulticastTTL { get; set; }

        void Start(bool joinMulticastGroup);

        void Send(Guid client, byte[] data);
        void Send(Guid client, MemoryStream data, bool keepAlive);
        void Send(Guid client, IPEndPoint remoteEndPoint, byte[] data);
        void Send(Guid client, IPEndPoint remoteEndPoint, MemoryStream data);

        void Send(Guid client, byte[] data, bool keepAlive);

        void Send(Guid client, string data, Encoding encoding, bool keepAlive);

        void Close(Guid client);

        IPEndPoint GetEndPoint();

        void Remove(Guid client);

        void Close();

        void HeartBeat();

        void JoinMulticastGroup(IPAddress group);

        void LeaveMulticastGroup(IPAddress group);
    }
}
