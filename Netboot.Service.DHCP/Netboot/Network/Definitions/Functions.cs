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

using Netboot.Network.Client;
using Netboot.Network.Client.RBCP;
using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using System.Buffers.Binary;
using System.Net;
using System.Text;
using YamlDotNet.Serialization;

namespace Netboot.Network.Definitions
{
	public static partial class Functions
	{
       


        public static DHCPOption GenerateBootServersList(Dictionary<string, BootServer> serverlist)
		{
			var ipBlock = 0;
			#region "How many bytes does we need for the IPAdresses"
			foreach (var server in serverlist)
			{
				var addresses = server.Value.Addresses;
				if (!addresses.Any() || string.IsNullOrEmpty(server.Value.Hostname))
					continue;

				var ipLength = server.Value.Addresses.First().GetAddressBytes().Length;

				ipBlock += server.Value.Addresses.Count;
			}
			#endregion

			var serverListBlock = new byte[sizeof(byte) + ipBlock * 4 + sizeof(ushort)];
			var offset = 0;

			foreach (var server in serverlist)
			{
				var addresses = server.Value.Addresses;
				if (!addresses.Any() || string.IsNullOrEmpty(server.Value.Hostname))
					continue;

				#region "Bootserver Type (Option)"
				var typeBytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16BigEndian(typeBytes, (ushort)server.Value.Type);
				Array.Copy(typeBytes, 0, serverListBlock, offset, typeBytes.Length);
				offset += typeBytes.Length;
				#endregion

				#region "IPAddress count (Length)"
				serverListBlock[offset] = Convert.ToByte(addresses.Count());
				offset += sizeof(byte);
				#endregion

				#region "Addresses"
				foreach (var addr in addresses)
				{
					var addrBytes = addr.GetAddressBytes();
					Array.Copy(addrBytes, 0, serverListBlock, offset, addrBytes.Length);
					offset += addrBytes.Length;
				}
				#endregion
			}

			return new((byte)PXEVendorEncOptions.BootServer, serverListBlock);
		}

		public static DHCPOption GenerateBootMenue(Dictionary<string, BootServer> servers)
		{
			#region Setup the Menue itself...
			var bootmenue = new List<BootMenueEntry>
			{
				new(0, "Local Boot")
			};

			foreach (var server in servers)
			{
				if (!server.Value.Addresses.Any() || string.IsNullOrEmpty(server.Value.Hostname))
					continue;

				bootmenue.Add(new((ushort)server.Value.Type, server.Value.Hostname));
			}
			#endregion

			var length = 0;
			foreach (var entry in bootmenue)
				length += entry.Description.Length + sizeof(ushort);

			var menuebuffer = new byte[length + 3];
			var offset = 0;

			foreach (var entry in bootmenue)
			{
				#region "Option"
				var typeBytes = new byte[sizeof(ushort)];
				BinaryPrimitives.WriteUInt16BigEndian(typeBytes, entry.Id);
				Array.Copy(typeBytes, 0, menuebuffer, offset, typeBytes.Length);
				offset += typeBytes.Length;
				#endregion

				#region "Length"
				menuebuffer[offset] = Convert.ToByte(entry.Description.Length);
				offset += sizeof(byte);
				#endregion

				#region "Description"
				var descBytes = Encoding.ASCII.GetBytes(entry.Description);
				Array.Copy(descBytes, 0, menuebuffer, offset, descBytes.Length);
				offset += descBytes.Length;
				#endregion
			}

			return new((byte)PXEVendorEncOptions.BootMenue, menuebuffer);
		}

		public static List<IPAddress> DNSLookup(string hostname)
			=> Dns.GetHostAddresses(hostname).ToList();

		public static DHCPOption GenerateBootMenuePrompt(byte timeout)
		{
			var prompt = Encoding.ASCII.GetBytes( timeout == byte.MaxValue ? "Select Server..." :
				"Press [F8] to boot from Network or [esc] to cancel...");
			
			var promptbuffer = new byte[1 + prompt.Length];
			var offset = 0;

			#region "Timeout"
			promptbuffer[offset] = timeout;
			offset += sizeof(byte);
			#endregion

			#region "Prompt"
			Array.Copy(prompt, 0, promptbuffer, offset, prompt.Length);
			#endregion

			return new((byte)PXEVendorEncOptions.MenuPrompt, promptbuffer);
		}

	}
}
