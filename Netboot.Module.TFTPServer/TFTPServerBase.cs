using Netboot.Common;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.System;
using Netboot.Module.TFTPServer.Event;
using System.Net;
using System.Xml;


namespace Netboot.Module.TFTPServer
{
	public class TFTPServerBase : IManager
	{
		private Dictionary<string, ITFTPClient> Clients { get; set; } = [];

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
                if (!NetbootBase.NetworkManager.ServerManager.HasSocket(e.Server, e.Socket))
                    return;

				var requestPacket = new TFTPPacket(e.Request);
				var endpoint = NetbootBase.NetworkManager.ServerManager.GetClientEndPoint(e.Server, e.Socket, e.Client);
				var clientId = endpoint.Address.ToString();

				switch (requestPacket.TFTPOPCode)
                {
                    case TFTPOPCodes.RRQ:
						if (Clients.ContainsKey(clientId))
                            Clients.Remove(clientId); 

						Clients.Add(clientId, new TFTPClient(false, clientId, e.Client, e.Server, e.Socket, requestPacket, endpoint));
						
						Handle_Read_Request(clientId);
                        break;
                    case TFTPOPCodes.ACK:
                        Handle_ACK_Request(clientId);
                        break;
                    case TFTPOPCodes.ERR:
                        Handle_Error_Request(clientId);
                        break;
                    default:
                        return;
                }

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

        public void Bootstrap(XmlNode xml)
        {

        }


        public void Close()
		{
		}

		public void Dispose()
		{
            Clients.Clear();
		}

		public void Handle_Listener_Request(Guid server, Guid socket, Guid client, MemoryStream memoryStream)
		{
			ListenerRequestReceived?.Invoke(this, new(memoryStream, server, socket, client));
		}

        public void Handle_Read_Request(string clientid)
        {
			NetbootBase.Log("I", "TFTPServer", string.Format("Got RRQ-Request from client: {0} (Options: {1})",
                Clients[clientid].RemoteEndpoint, string.Join(',', Clients[clientid].Request.Options.Keys.ToList())));
			
            if (!Clients[clientid].Request.Options.ContainsKey("file"))
            {
                Clients.Remove(clientid);
                return;
            }

            if (string.IsNullOrEmpty(Clients[clientid].Request.Options["file"]))
            {
				Clients[clientid].Response = new TFTPPacket(TFTPOPCodes.ERR)
				{
					ErrorCode = TFTPErrorCode.AccessViolation,
					ErrorMessage = Clients[clientid].FileName
				};

				Clients[clientid].Response.CommitOptions();

				NetbootBase.NetworkManager.ServerManager.Send(Clients[clientid].Server, Clients[clientid].Socket,
					Clients[clientid].Client, Clients[clientid].RemoteEndpoint, Clients[clientid].Response.Buffer.GetBuffer());

				Clients.Remove(clientid);
                return;
            }
            
            Clients[clientid].CloseFile();

            Clients[clientid].PacketBacklog.Clear();
            Clients[clientid].FileName = Filesystem.Resolve(Clients[clientid].Request.Options["file"]);

            var fileExists = Clients[clientid].OpenFile();

            Clients[clientid].Response = new TFTPPacket(!fileExists ? TFTPOPCodes.ERR : TFTPOPCodes.OCK);

            if (!fileExists)
            {
                Clients[clientid].Response.ErrorCode = TFTPErrorCode.FileNotFound;
                Clients[clientid].Response.ErrorMessage = Clients[clientid].FileName;

                NetbootBase.Log("E", "TFTPServer", string.Format("File not found: {0}", Clients[clientid].FileName));
            }
            else
            {
                if (Clients[clientid].Request.Options.ContainsKey("blksize"))
                    Clients[clientid].BlockSize = ushort.Parse(Clients[clientid].Request.Options["blksize"]);

                if (Clients[clientid].Request.Options.ContainsKey("windowsize"))
                    Clients[clientid].WindowSize = byte.Parse(Clients[clientid].Request.Options["windowsize"]);

                if (Clients[clientid].Request.Options.ContainsKey("msftwindow"))
                    Clients[clientid].MSFTWindow = ushort.Parse(Clients[clientid].Request.Options["msftwindow"]);

                Clients[clientid].CurrentBlock = 0;

                if (Clients[clientid].Request.Options.ContainsKey("tsize"))
                    Clients[clientid].Response.Options.Add("tsize", string.Format("{0}", Clients[clientid].BytesToRead));

                if (Clients[clientid].Request.Options.ContainsKey("blksize"))
                    Clients[clientid].Response.Options.Add("blksize", string.Format("{0}", Clients[clientid].BlockSize));

                if (Clients[clientid].Request.Options.ContainsKey("windowsize"))
                    Clients[clientid].Response.Options.Add("windowsize", string.Format("{0}", Clients[clientid].WindowSize));

                if (Clients[clientid].Request.Options.ContainsKey("msftwindow"))
                   Clients[clientid].Response.Options.Add("msftwindow", string.Format("{0}", 27182));
            }

			Clients[clientid].Response.CommitOptions();

			NetbootBase.NetworkManager.ServerManager.Send(Clients[clientid].Server, Clients[clientid].Socket,
				Clients[clientid].Client, Clients[clientid].RemoteEndpoint, Clients[clientid].Response.Buffer.GetBuffer());
		}

        public void Handle_ACK_Request(string clientid)
        {
            if (!Clients.ContainsKey(clientid))
                return;

            if (Clients[clientid].Request.Block != Clients[clientid].CurrentBlock)
                Clients[clientid].ResetState(Clients[clientid].Request.Block);

            if (Clients[clientid].Request.Options.ContainsKey("NextWindow"))
                Clients[clientid].WindowSize = Clients[clientid].Request.NextWindow;

            Clients[clientid].OpenFile();

            for (var i = 0; i < Clients[clientid].WindowSize; i++)
            {
                var chunk = Clients[clientid].ReadChunk();
				using (Clients[clientid].Response = new TFTPPacket(TFTPOPCodes.DAT))
                {
                    Clients[clientid].CurrentBlock++;

                    Clients[clientid].Response.Block = Clients[clientid].CurrentBlock;
                    Clients[clientid].Response.Data = chunk;
                    Clients[clientid].Response.CommitOptions();

                    AddEntryToPacketBacklog?.Invoke(this, new(clientid,
                        new(Clients[clientid].BytesRead, Clients[clientid].BytesToRead,
                            Clients[clientid].Response.Block)));

					NetbootBase.NetworkManager.ServerManager.Send(Clients[clientid].Server, Clients[clientid].Socket,
						Clients[clientid].Client, Clients[clientid].RemoteEndpoint, Clients[clientid].Response.Buffer.GetBuffer());
				}

                if (Clients[clientid].BytesToRead == Clients[clientid].BytesRead)
                    break;
            }

            if (Clients[clientid].BytesToRead == Clients[clientid].BytesRead)
                Clients.Remove(clientid);
        }

        public void Handle_Error_Request(string clientid)
        {
            NetbootBase.Log("E", "TFTPServer", string.Format("({0}): {1}",
                Clients[clientid].Request.ErrorCode, Clients[clientid].Request.ErrorMessage));
        }
    }
}
