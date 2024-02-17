using Netboot.Common;
using Netboot.Network.Interfaces;
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
			FileStream.Close();
			FileStream.Dispose();
		}
	}
}
