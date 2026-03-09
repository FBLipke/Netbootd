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

namespace Netboot.Common.Network
{
	public class HTTPRequestReceivedEventArgs : EventArgs
	{
		public NetbootHttpContext Context { get; private set; }

		public Guid Server { get; private set; }

		public Guid Socket { get; private set; }

		public Guid Client { get; private set; }

		public bool MediaRequest { get; private set; }
		public HTTPRequestReceivedEventArgs(
  Guid server,
  Guid socket,
  Guid client,
  bool icyData,
  NetbootHttpContext context)
		{
			MediaRequest = icyData;
			Server = server;
			Socket = socket;
			Client = client;
			Context = context;
		}
	}
}
