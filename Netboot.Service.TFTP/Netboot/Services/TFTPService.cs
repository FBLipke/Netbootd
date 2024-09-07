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

using Netboot.Common;
using Netboot.Network.Client;
using Netboot.Network.EventHandler;
using Netboot.Network.Interfaces;
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

			AddEntryToPacketBacklog += (sender, e) => {
				var client = e.Client;

                if (!Clients.ContainsKey(client))
					return;

				var block = e.TFTPPacketBacklogEntry.Block;

                if (!Clients[client].PacketBacklog.ContainsKey(block))
                    Clients[client].PacketBacklog.Add(block, e.TFTPPacketBacklogEntry);
            };
        }

		delegate void AddEntryToPacketBacklogEventHandler(object sender, PacketBacklogEventArgs e);
		event AddEntryToPacketBacklogEventHandler AddEntryToPacketBacklog;

		public string ServiceType { get; }

		public string RootPath { get; set; }

		public Dictionary<string, TFTPClient> Clients { get; set; } = [];

		public List<ushort> Ports { get; set; } = [];

		public event IService.AddServerEventHandler? AddServer;
		public event IService.ServerSendPacketEventHandler? ServerSendPacket;
        public event IService.PrintMessageEventHandler? PrintMessage;

        public void Dispose()
		{
			foreach (var client in Clients.Values.ToList())
				client.Dispose();

			Ports.Clear();
		}

		private void RemoveClient(string id)
		{
			if (Clients.ContainsKey(id))
			{
				Clients[id].Dispose();
				Clients.Remove(id);
			}
		}

		void AddClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverId, Guid socketId)
		{
			if (!Clients.TryGetValue(clientId, out TFTPClient? value))
				Clients.Add(clientId, new(clientId, serviceType, remoteEndpoint, serverId, socketId));
			else
			{
				value.RemoteEntpoint = remoteEndpoint;
			}
		}

		public void Handle_DataReceived(object sender, DataReceivedEventArgs e)
		{
			var requestPacket = new TFTPPacket(e.ServiceType, e.Packet);
            var clientid = e.RemoteEndpoint.Address.ToString();

            switch (requestPacket.TFTPOPCode)
			{
				case TFTPOPCodes.RRQ:
					RemoveClient(clientid);
					AddClient(clientid, e.ServiceType, e.RemoteEndpoint, e.ServerId, e.SocketId);
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
			if (!packet.Options.ContainsKey("file"))
			{
				Clients.Remove(client);
				return;
			}

			Clients[client].CloseFile();

            Clients[client].PacketBacklog.Clear();
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
			RootPath = NetbootBase.Platform.TFTPRoot;

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

				using (var response = new TFTPPacket(ServiceType, TFTPOPCodes.DAT))
				{
					Clients[client].CurrentBlock++;

					response.Block = Clients[client].CurrentBlock;
					response.Data = data;
					response.CommitOptions();

					AddEntryToPacketBacklog?.Invoke(this, new(client,
						new(Clients[client].BytesRead, Clients[client].BytesToRead, response.Block)));

                    ServerSendPacket?.Invoke(this, new(ServiceType, server, socket, response, Clients[client]));
				}

                if (Clients[client].BytesToRead == Clients[client].BytesRead)
                    break;
            }

            if (Clients[client].BytesToRead == Clients[client].BytesRead)
				RemoveClient(client);
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}
	}
}
