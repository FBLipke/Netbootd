namespace Netboot.Network.Interfaces
{
    public interface IServer : IDisposable
    {
        void Start();
        void Stop();
        void Send(Guid socketId, IPacket packet, IClient client);
    }
}
