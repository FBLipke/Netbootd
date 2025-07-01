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
using Netboot.Service.TFTP;
using System.Net;

namespace Netboot.Network.Client
{
	public class TFTPClient : BaseClient
	{
		public TFTPClient(bool testClient, string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
			: base(testClient, clientId, serviceType, remoteEndpoint, serverid, socketId)
		{ 
			BytesRead = 0;
		}

		public Dictionary<ushort, TFTPPacketBacklogEntry> PacketBacklog { get; set; } = [];

		public ushort BlockSize { get; set; } = 4096;

		public ushort CurrentBlock { get; set; } = 0;

		public ushort TotalBlocks { get; set; } = ushort.MinValue;

		public long BytesToRead { get; set; } = long.MinValue;

		public long BytesRead { get; set; } = long.MinValue;

		private bool isOpen = false;

		public byte WindowSize { get; set; } = 1;

		public ushort MSFTWindow { get; set; } = 31416;

		public string FileName { get; set; } = string.Empty;

		public FileStream FileStream { get; internal set; }

		public bool OpenFile()
		{
			if (isOpen)
				return true;

			try
			{
				var fil = new FileInfo(Functions.ReplaceSlashes(FileName));
				if (fil.Exists)
				{
					FileStream = new FileStream(fil.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 2 << 64);
					BytesToRead = FileStream.Length;
					BytesRead = 0;
					FileStream.Position = 0;
					isOpen = true;
				}
				else
					return false;

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

			if ((BytesToRead - BytesRead) <= chunksize)
				chunksize = (ushort)(BytesToRead - BytesRead);

			var buffer = new byte[chunksize];

			if (FileStream != null)
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

		public override void Dispose()
		{
			CloseFile();
			FileStream?.Dispose();
			PacketBacklog.Clear();
		}
	}
}
