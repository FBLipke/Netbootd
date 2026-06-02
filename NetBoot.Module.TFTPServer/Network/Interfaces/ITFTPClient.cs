using System.Net;

namespace Netboot.Module.TFTPServer
{
    public interface ITFTPClient : IDisposable
    {
        public Dictionary<ushort, TFTPPacketBacklogEntry> PacketBacklog { get; set; }

        public ushort BlockSize { get; set; }

        public ushort CurrentBlock { get; set; }

        public ushort TotalBlocks { get; set; }

        public long BytesToRead { get; set; }

        public long BytesRead { get; set; }

        public bool isOpen { get; set; }

        public byte WindowSize { get; set; }

        public ushort MSFTWindow { get; set; }

        public IPEndPoint RemoteEndpoint { get; set; }

        public string FileName { get; set; }

        public FileStream FileStream { get; set; }

        public Guid Id { get; set; }

        Guid Socket { get; set; }

        Guid Server { get; set; }

        Guid Client { get; set; }

        bool OpenFile();

        public TFTPPacket Request { get; set; }

        public TFTPPacket Response { get; set; }

        void ResetState(ushort block);

        byte[] ReadChunk();

        void CloseFile();
    }
}
