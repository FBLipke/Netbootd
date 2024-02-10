using Netboot.Common.Network.Interfaces;
using NetBoot.Common.Netboot.Common.Network.Definitions;
using NetBoot.Common.Netboot.Common.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common.Network.Client
{
    public class BaseClient : IClient
    {
        Guid SocketId;
		Guid ServerId;
		public ServerType ServerType;
        public Guid ClientId = Guid.Empty;

        public BaseClient(Guid clientId, ServerType serverType, Guid serverid, Guid socketId) {
            ServerType = serverType;
            SocketId = socketId;
			ServerId = serverid;
			ClientId = clientId;
        }

		public void Close()
		{
		}

		public void Dispose()
		{
		}
	}
}
