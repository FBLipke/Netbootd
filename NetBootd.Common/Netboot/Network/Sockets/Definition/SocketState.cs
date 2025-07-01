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

using System.Net.Sockets;

namespace Netboot.Network.Sockets
{
    internal class SocketState : IDisposable
    {
        public Socket? socket;
        public Memory<byte> buffer;
        private bool IsDisposed;

        public SocketState()
        {
        }

        public void Close()
        {
            socket.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    socket?.Dispose();
                }

                socket = null;
                IsDisposed = true;
            }
        }
    }
}
