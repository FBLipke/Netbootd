using Netboot.Common;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.System;
using Netboot.Module.TFTPServer.Event;
using System.Net;
using System.Reflection;

namespace Netboot.Module.TFTPServer
{
	public class TFTPServerBase : IManager
	{
		private Dictionary<Guid, ITFTPClient> Clients { get; set; } = [];

		private Filesystem Filesystem { get; set; }


		private IDatabase Database { get; set; }

        delegate void AddEntryToPacketBacklogEventHandler(object sender, PacketBacklogEventArgs e);
        event AddEntryToPacketBacklogEventHandler AddEntryToPacketBacklog;

        public delegate void ListenerRequestReceivedEventHandler
			(object sender, ListenerRequestReceivedEventArgs e);

		public event ListenerRequestReceivedEventHandler
			ListenerRequestReceived;

		public TFTPServerBase(Filesystem filesystem, IDatabase database)
		{
			Database = database;
			Filesystem = filesystem;

            ListenerRequestReceived += (sender, e) => {

            };
        }

		public void Start()
		{
		}

		public void Stop()
		{
		}

		public void HeartBeat()
		{
		}

		public void Bootstrap()
		{
		}

		public void Close()
		{
		}

		public void Dispose()
		{
		}

		public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
		{
			ListenerRequestReceived?.Invoke(this, new ListenerRequestReceivedEventArgs(memoryStream, server, socket, client));
		}

        public void Handle_Read_Request(Guid server, Guid socket, Guid client, TFTPPacket packet)
        {
            if (!packet.Options.ContainsKey("file"))
            {
                Clients.Remove(client);
                return;
            }

            Clients[client].CloseFile();

            Clients[client].PacketBacklog.Clear();
            Clients[client].FileName = Functions.ReplaceSlashes(Path.Combine(Filesystem.Root, packet.Options["file"]));

            var fileExists = Clients[client].OpenFile();

            var response = new TFTPPacket(!fileExists ? TFTPOPCodes.ERR : TFTPOPCodes.OCK);

            if (!fileExists)
            {
                response.ErrorCode = TFTPErrorCode.FileNotFound;
                response.ErrorMessage = Clients[client].FileName;

                Console.WriteLine("[E] TFTP: File not found: {0}", Clients[client].FileName);
            }
            else
            {
                if (packet.Options.ContainsKey("blksize"))
                    Clients[client].BlockSize = ushort.Parse(packet.Options["blksize"]);

                if (packet.Options.ContainsKey("windowsize"))
                    Clients[client].WindowSize = byte.Parse(packet.Options["windowsize"]);

                if (packet.Options.ContainsKey("msftwindow"))
                    Clients[client].MSFTWindow = ushort.Parse(packet.Options["msftwindow"]);

                Clients[client].CurrentBlock = 0;

                if (packet.Options.ContainsKey("tsize"))
                    response.Options.Add("tsize", string.Format("{0}", Clients[client].BytesToRead));

                if (packet.Options.ContainsKey("blksize"))
                    response.Options.Add("blksize", string.Format("{0}", Clients[client].BlockSize));

                if (packet.Options.ContainsKey("windowsize"))
                    response.Options.Add("windowsize", string.Format("{0}", Clients[client].WindowSize));

                if (packet.Options.ContainsKey("msftwindow"))
                    response.Options.Add("msftwindow", string.Format("{0}", 27182));

                response.CommitOptions();
            }

            //ServerSendPacket?.Invoke(this, new(server, socket, response, Clients[client]));
        }

        public void Handle_ACK_Request(Guid server, Guid socket, Guid client, TFTPPacket packet)
        {
            if (!Clients.ContainsKey(client))
                return;

            if (packet.Block != Clients[client].CurrentBlock)
                Clients[client].ResetState(packet.Block);

            if (packet.Options.ContainsKey("NextWindow"))
                Clients[client].WindowSize = packet.NextWindow;

            Clients[client].OpenFile();

            for (var i = 0; i < Clients[client].WindowSize; i++)
            {
                var data = Clients[client].ReadChunk();

                using (var response = new TFTPPacket(TFTPOPCodes.DAT))
                {
                    Clients[client].CurrentBlock++;

                    response.Block = Clients[client].CurrentBlock;
                    response.Data = data;
                    response.CommitOptions();

                    AddEntryToPacketBacklog?.Invoke(this, new(client,
                        new(Clients[client].BytesRead, Clients[client].BytesToRead, response.Block)));

                   // ServerSendPacket?.Invoke(this, new(server, socket, response, Clients[client]));
                }

                if (Clients[client].BytesToRead == Clients[client].BytesRead)
                    break;
            }

            if (Clients[client].BytesToRead == Clients[client].BytesRead)
                Clients.Remove(client);
        }

        public void Handle_Error_Request(Guid server, Guid socket, string client, TFTPPacket packet)
        {
            Console.WriteLine("[E] TFTP: ({0}): {1}!", packet.ErrorCode, packet.ErrorMessage);
        }
    }
}
