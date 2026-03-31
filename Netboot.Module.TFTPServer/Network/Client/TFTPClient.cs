/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Net;

namespace Netboot.Module.TFTPServer
{
    public class TFTPClient : ITFTPClient
    {
        public TFTPClient(bool testClient, string clientId, Guid client, Guid server, Guid socket, TFTPPacket request, IPEndPoint endpoint)
        {
            BytesRead = 0;
            Request = request;
            RemoteEndpoint = endpoint;
            Id = Client;

            Socket = socket;
            Server = server;
            Client = client;
        }

        public Dictionary<ushort, TFTPPacketBacklogEntry> PacketBacklog { get; set; } = [];

        public ushort BlockSize { get; set; } = 4096;

        public ushort CurrentBlock { get; set; } = 0;

        public ushort TotalBlocks { get; set; } = ushort.MinValue;

        public long BytesToRead { get; set; } = long.MinValue;

        public long BytesRead { get; set; } = long.MinValue;

        public bool isOpen { get; set; } = false;

        public byte WindowSize { get; set; } = 1;

        public IPEndPoint RemoteEndpoint { get; set; }

        public ushort MSFTWindow { get; set; } = 31416;

        public string FileName { get; set; } = string.Empty;

        public FileStream FileStream { get; set; }

        public Guid Id { get; set; }

        public Guid Socket { get; set; }

        public Guid Server { get; set; }

        public Guid Client { get; set; }

        public TFTPPacket Request { get; set; }

        public TFTPPacket Response { get; set; }

        public bool OpenFile()
        {
            if (isOpen)
                return true;

            try
            {
                var fil = new FileInfo(FileName);
                if (fil.Exists)
                {
                    FileStream = new FileStream(fil.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 2 << 64);
                    BytesToRead = FileStream.Length;
                    BytesRead = 0;
                    FileStream.Position = 0;
                    isOpen = true;
                }
                else
                    isOpen = false;

                return isOpen;
            }
            catch (Exception ex)
            {
                CloseFile();

                Console.WriteLine(ex);
                return isOpen;
            }
        }

        public void ResetState(ushort block)
        {
            if (PacketBacklog.ContainsKey(block))
            {
                BytesRead = PacketBacklog[block].BytesRead;
                BytesToRead = PacketBacklog[block].BytesToRead;
                CurrentBlock = PacketBacklog[block].Block;
            }
        }

        public byte[] ReadChunk()
        {
            var chunksize = BlockSize;

            if (BytesToRead - BytesRead <= chunksize)
                chunksize = (ushort)(BytesToRead - BytesRead);

            var buffer = new byte[chunksize];

            if (FileStream != null && isOpen)
            {
                FileStream.Seek(BytesRead, SeekOrigin.Begin);
                var readedBytes = FileStream.Read(buffer, 0, buffer.Length);
                BytesRead += readedBytes;
            }

            return buffer;
        }

        public void CloseFile()
        {
            FileStream?.Close();
        }

        public void Dispose()
        {
            CloseFile();
            FileStream?.Dispose();
            PacketBacklog.Clear();
        }
    }
}
