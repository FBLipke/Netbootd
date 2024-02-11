namespace Netboot.Network.EventHandler
{
    public class AddServerEventArgs
    {
        public string ServiceType { get; private set; }
        public List<ushort> Ports { get; private set; }

        public AddServerEventArgs(string type, List<ushort> ports)
        {
            ServiceType = type;
            Ports = ports;
        }
    }
}