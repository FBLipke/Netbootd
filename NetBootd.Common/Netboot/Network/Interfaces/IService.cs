using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;

namespace Netboot.Services.Interfaces
{
    public interface IService : IDisposable
    {
        delegate void AddServerEventHandler(object sender, AddServerEventArgs e);
        event AddServerEventHandler? AddServer;

        List<ushort> Ports { get; }

        string ServiceType { get; }

        Dictionary<Guid, IClient> Clients { get; set; }

        void Handle_DataReceived(object sender, DataReceivedEventArgs e);
        void Handle_DataSent(object sender, DataSentEventArgs e);

        void Start();
        void Stop();

        bool Initialize();
    }
}
