using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;

namespace Netboot.Services
{
    public class BaseService : IService
    {

        public BaseService(string serviceType) {
            ServiceType = serviceType;
        }

        public Dictionary<Guid, IClient> Clients { get; set; } = [];

        public List<ushort> Ports { get; }  = new List<ushort>();

        public string ServiceType { get; }

        public event IService.AddServerEventHandler? AddServer;

        public void Dispose()
        {
        }

        public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Handle_DataSent(object sender, DataSentEventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            AddServer.Invoke(this, new AddServerEventArgs(ServiceType, Ports));
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
