using System.Net;

namespace Netboot.Module.DHCPListener
{
    public class RegisterBootServiceEventArgs
    {
        public BootServerType Type { get; private set; }

        public string Description { get; private set; }

        public List<IPAddress> Addresses { get; private set; }

        public RegisterBootServiceEventArgs(BootServerType type, string description, List<IPAddress> addresses = null)
        {
            Type = type;
            Description = description;
            Addresses = addresses == null ? [] : addresses;
        }
    }
}