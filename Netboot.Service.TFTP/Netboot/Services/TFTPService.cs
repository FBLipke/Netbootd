using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
using Netboot.Services.Interfaces;
using System.Xml;

namespace Netboot.Service.TFTP
{
    public class TFTPService : IService
    {
        public TFTPService(string serviceType)
        {
            ServiceType = serviceType;
        }

        public string ServiceType { get; }

        public Dictionary<string, IClient> Clients { get; set; } = [];

        public List<ushort> Ports { get; set; } = [];

        public event IService.AddServerEventHandler? AddServer;
        public event IService.ServerSendPacketEventHandler? ServerSendPacket;

        public void Dispose()
        {
        }

        public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
        {
        }

        public void Handle_DataSent(object sender, DataSentEventArgs e)
        {
        }

        public void Heartbeat()
        {
        }

        public bool Initialize(XmlNode xmlConfigNode)
        {
            var ports = xmlConfigNode.Attributes.GetNamedItem("port").Value.Split(',').ToList();
            if (ports.Count > 0)
            {
                Ports.AddRange(from port in ports
                               select ushort.Parse(port.Trim()));
            }

            AddServer?.Invoke(this, new(ServiceType, Ports));
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
