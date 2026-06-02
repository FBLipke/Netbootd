namespace Netboot.Module.DHCPListener.Event
{
    public class ListenerRequestReceivedEventArgs
    {
        public MemoryStream Request { get; private set; }

        public Guid Server { get; private set; }

        public Guid Socket { get; private set; }

        public Guid Client { get; private set; }

        public ListenerRequestReceivedEventArgs(MemoryStream request, Guid server, Guid socket, Guid client)
        {
            Request = request;
            Server = server;
            Socket = socket;
            Client = client;
        }

    }
}