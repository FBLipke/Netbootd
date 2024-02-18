using Netboot.Common;
using Netboot.Network.Client;
using Netboot.Network.EventHandler;
using Netboot.Service.TFTP.Netboot.Network;
using Netboot.Service.TFTP.Netboot.Network.Packet;
using Netboot.Services.Interfaces;
using System.Net;
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

        public string RootPath { get; set; }

        public Dictionary<string, TFTPClient> Clients { get; set; } = [];

        public List<ushort> Ports { get; set; } = [];

        public event IService.AddServerEventHandler? AddServer;
        public event IService.ServerSendPacketEventHandler? ServerSendPacket;

        public void Dispose()
        {
			foreach (var client in Clients.Values)
				client.Dispose();

			Ports.Clear();
		}


		void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
		{
			if (!Clients.ContainsKey(clientId))
				Clients.Add(clientId, new TFTPClient(clientId, serviceType, remoteEndpoint, serverId, socketId));
			else
			{
				Clients[clientId].RemoteEntpoint = remoteEndpoint;
			}
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
        {


            var clientid = e.RemoteEndpoint.Address.ToString();
            AddClient(clientid, e.ServiceType, e.RemoteEndpoint, e.ServerId, e.SocketId);

            var requestPacket = new TFTPPacket(e.ServiceType, e.Packet);

            switch (requestPacket.TFTPOPCode)
            {
                case TFTPOPCodes.RRQ:
					Console.WriteLine("[I] Got TFTP Request from: {0}", e.RemoteEndpoint);
                    Handle_Read_Request(e.ServerId, e.SocketId, clientid, requestPacket);
                    break;
                case TFTPOPCodes.ACK:
					Handle_ACK_Request(e.ServerId, e.SocketId, clientid, requestPacket);
					break;
                case TFTPOPCodes.ERR:
					Handle_Error_Request(e.ServerId, e.SocketId, clientid, requestPacket);
					break;
				default:
                    break;
            }
        }

        public void Handle_Read_Request(Guid server, Guid socket, string client, TFTPPacket packet)
        {
            if (packet.Options.ContainsKey("tsize"))
                Clients[client].BytesToRead = long.Parse(packet.Options["tsize"]);

			if (!packet.Options.ContainsKey("file"))
			{
				Clients.Remove(client);
				return;
			}

            Clients[client].FileName = Functions.ReplaceSlashes(Path.Combine(RootPath, packet.Options["file"]));

            var fileExists = Clients[client].OpenFile();

			var response = new TFTPPacket(ServiceType, !fileExists ? TFTPOPCodes.ERR : TFTPOPCodes.OCK);

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
					Clients[client].WindowSize = ushort.Parse(packet.Options["windowsize"]);

				Clients[client].CurrentBlock = 0;

				if (packet.Options.ContainsKey("tsize"))
					response.Options.Add("tsize", string.Format("{0}", Clients[client].BytesToRead));

				if (packet.Options.ContainsKey("blksize"))
					response.Options.Add("blksize", string.Format("{0}", Clients[client].BlockSize));

				if (packet.Options.ContainsKey("windowsize"))
					response.Options.Add("windowsize", string.Format("{0}", Clients[client].WindowSize));

				response.CommitOptions();
			}

            ServerSendPacket?.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));
		}

		public void Handle_DataSent(object sender, DataSentEventArgs e)
        {
        }

        public void Handle_Error_Request(Guid server, Guid socket, string client, TFTPPacket packet)
        {
            Console.WriteLine("[E] TFTP: ({0}): {1}!", packet.ErrorCode, packet.ErrorMessage);
        }

		public void Heartbeat()
        {
        }

        public bool Initialize(XmlNode xmlConfigNode)
        {
            RootPath = Path.Combine(NetbootBase.WorkingDirectory, "TFTPRoot");

			var ports = xmlConfigNode.Attributes.GetNamedItem("port").Value.Split(',').ToList();
            if (ports.Count > 0)
            {
                Ports.AddRange(from port in ports
                    select ushort.Parse(port.Trim()));
            }

            AddServer?.Invoke(this, new(ServiceType, Ports));
            return true;
        }

		public void Handle_ACK_Request(Guid server, Guid socket, string client, TFTPPacket packet)
		{
			if (packet.Block != Clients[client].CurrentBlock)
			{
				Clients[client].CloseFile();

				if (Clients.ContainsKey(client))
					Clients.Remove(client);
				return;
			}

			Clients[client].OpenFile();

			for (var i = 0; i < Clients[client].WindowSize; i++)
			{
				var data = Clients[client].ReadChunk(out var readedBytes);

				Clients[client].BytesRead += readedBytes;

				using (var response = new TFTPPacket(ServiceType, TFTPOPCodes.DAT))
				{
					Clients[client].CurrentBlock++;

					response.Block = Clients[client].CurrentBlock;
					response.Data = data;
					response.CommitOptions();

					ServerSendPacket?.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));
				}

				if (data.Length < Clients[client].BlockSize)
					break;

				if (Clients[client].BytesRead == Clients[client].BytesToRead)
					break;
			}
			
			Clients[client].CloseFile();

			if (Clients[client].BytesRead == Clients[client].BytesToRead)
				Clients.Remove(client);
		}

		public void Start()
        {
        }

        public void Stop()
        {
		}
	}
}
