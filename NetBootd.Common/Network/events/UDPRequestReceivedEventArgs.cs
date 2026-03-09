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
	public class UDPRequestReceivedEventArgs : EventArgs
	{
		public Guid Server { get; private set; }

		public Guid Socket { get; private set; }

		public Guid Client { get; private set; }

		public MemoryStream Data { get; private set; }

		public UDPRequestReceivedEventArgs(
  Guid server,
  Guid socket,
  Guid client,
  byte[] data)
		{
			Server = server;
			Socket = socket;
			Client = client;
			Data = new MemoryStream(data, false);
		}
	}
}
