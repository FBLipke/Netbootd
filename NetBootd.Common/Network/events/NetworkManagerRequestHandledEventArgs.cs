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

using Netboot.Common.Network.HTTP;
using System;

namespace Netboot.Common.Network.Sockets
{
	public class NetworkManagerRequestHandledEventArgs : EventArgs
	{
		public Guid Server { get; private set; }

		public Guid Socket { get; private set; }

		public Guid Client { get; private set; }

		public bool KeepAlive { get; private set; }

		public HttpResponse Response { get; private set; }

		public NetworkManagerRequestHandledEventArgs(
		  Guid server,
		  Guid socket,
		  Guid client,
		  HttpResponse response)
		{
			Server = server;
			Socket = socket;
			Client = client;
			Response = response;
		}
	}
}
