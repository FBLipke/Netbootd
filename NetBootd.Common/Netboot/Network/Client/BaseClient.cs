﻿/*
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

using Netboot.Network.Interfaces;
using System.Net;

namespace Netboot.Network.Client
{
	public class BaseClient : IClient, IDisposable
	{
		private bool testClient { get; } = false;

		public Guid SocketId { get; set; }
		
		public Guid ServerId { get; set; }

		public string ServiceType { get; set; }
		
		public string ClientId { get; set; }

		public DateTime CreationTime { get; set; }

		public DateTime LastUpdate { get; set; }

		public IPEndPoint RemoteEndpoint { get; set; }

		public BaseClient(bool test, string clientId, string serviceType,
			IPEndPoint remoteEndpoint, Guid serverid, Guid socketId)
		{
			testClient = test;
			ServiceType = serviceType;
			SocketId = socketId;
			ServerId = serverid;
			ClientId = clientId;
			RemoteEndpoint = remoteEndpoint;
			CreationTime = DateTime.Now;
			LastUpdate = DateTime.Now;
		}

		public void UpdateTimestamp()
		{
			LastUpdate = DateTime.Now;
		}

		public void Close()
		{
		}

		public virtual void Dispose()
		{
		}

		public virtual void Heartbeat()
		{
		}
	}
}
