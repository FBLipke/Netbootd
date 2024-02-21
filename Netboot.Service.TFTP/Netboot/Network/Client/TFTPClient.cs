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
using System.Net;

namespace Netboot.Network.Client
{
	public class TFTPClient : BaseClient
	{
		public TFTPClient(string clientId, string serviceType, IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
			: base(clientId, serviceType, remoteEndpoint, serverid, socketId)
		{
			BytesRead = 0;
		}

		public ushort BlockSize { get; set; } = 4096;

		public ushort CurrentBlock { get; set; } = 0;

		public ushort TotalBlocks { get; set; } = ushort.MinValue;

		public long BytesToRead { get; set; } = long.MinValue;

		public long BytesRead { get; set; } = long.MinValue;

		public ushort WindowSize { get; set; } = 1;

		public string FileName { get; set; } = string.Empty;

		public FileStream FileStream { get; internal set; }

		public bool OpenFile()
		{
			try
			{
				var fil = new FileInfo(Functions.ReplaceSlashes(FileName));
				if (fil.Exists)
				{
					FileStream = new FileStream(fil.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
					BytesToRead = FileStream.Length;
					FileStream.Position = 0;

					if (BytesRead == BytesToRead)
						CloseFile();

					return true;
				}
				else
					return false;
			}
			catch (Exception ex)
			{
				if (BytesRead == BytesToRead)
					CloseFile();

				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public byte[] ReadChunk(out int readedBytes)
		{
			var chunksize = BlockSize;

			if (BytesToRead - BytesRead < BlockSize)
				chunksize = (ushort)(BytesToRead - BytesRead);

			var buffer = new byte[chunksize];

			FileStream.Seek(BytesRead, SeekOrigin.Current);
			readedBytes = FileStream.Read(buffer, 0, buffer.Length);

			return buffer;
		}

		public void CloseFile()
		{
			if (FileStream == null)
				return;

			FileStream.Close();
			FileStream.Dispose();
		}

		public override void Dispose()
		{
			FileStream?.Close();
			FileStream?.Dispose();
		}
	}
}
