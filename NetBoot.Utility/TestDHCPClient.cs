/*
Test DHCP Client - Embedded in Netbootd
Sends DHCP Discover and shows responses
*/

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetBoot.Utility
{
    public class TestDHCPClient
    {
        public static void Run(string targetIP = "10.232.128.101", int port = 67)
        {
            Console.WriteLine($"[TEST] DHCP Client starting...");
            Console.WriteLine($"[TEST] Target: {targetIP}:{port}");

            var client = new UdpClient();
            try
            {
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            catch { }
            
            // DHCP Discover broadcast
            var discover = CreateDHCPDiscover();
            var target = new IPEndPoint(IPAddress.Parse(targetIP), port);
            
            Console.WriteLine($"[TEST] Sending DHCP Discover...");
            client.Send(discover, discover.Length, target);

            // Wait for response
            client.Client.ReceiveTimeout = 5000;
            try
            {
                var response = client.Receive(ref target);
                Console.WriteLine($"[TEST] Got {response.Length} bytes from {target}");
                ParseDHCPResponse(response);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[TEST] No response: {ex.Message}");
            }
            finally
            {
                client?.Close();
                client?.Dispose();
            }
        }

        private static byte[] CreateDHCPDiscover()
        {
            // Minimal DHCP Discover packet
            var packet = new byte[548];
            
            // Operation (1 = Request)
            packet[0] = 1;
            
            // Hardware Type (1 = Ethernet)
            packet[1] = 1;
            
            // Hardware Address Length
            packet[2] = 6;
            
            // Hops
            packet[3] = 0;
            
            // Transaction ID (random)
            var rand = new Random();
            for (int i = 4; i < 8; i++)
                packet[i] = (byte)rand.Next(256);
            
            // Seconds elapsed
            packet[8] = 0;
            packet[9] = 0;
            
            // Flags (0x8000 = broadcast)
            packet[10] = 0x80;
            packet[11] = 0x00;
            
            // CIADDR (client IP)
            for (int i = 12; i < 16; i++)
                packet[i] = 0;
            
            // YIADDR (your IP - server fills this)
            for (int i = 16; i < 20; i++)
                packet[i] = 0;
            
            // SIADDR (server IP)
            for (int i = 20; i < 24; i++)
                packet[i] = 0;
            
            // GIADDR (gateway IP)
            for (int i = 24; i < 28; i++)
                packet[i] = 0;
            
            // CHADDR (client hardware address - fake MAC)
            var mac = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
            for (int i = 0; i < 6; i++)
                packet[28 + i] = mac[i];
            for (int i = 34; i < 44; i++)
                packet[i] = 0;
            
            // Magic cookie (0x63825363)
            packet[236] = 0x63;
            packet[237] = 0x82;
            packet[238] = 0x53;
            packet[239] = 0x63;
            
            // DHCP Message Type (53, 1, 1 = Discover)
            packet[240] = 53;  // Option
            packet[241] = 1;    // Len
            packet[242] = 1;   // Value = Discover
            
            // DHCP Parameter Request List (55)
            packet[243] = 55;
            packet[244] = 4;
            packet[245] = 1;   // Subnet Mask
            packet[246] = 3;    // Router
            packet[247] = 6;    // DNS
            packet[248] = 15;   // Domain Name

            // Option 60 (Vendor Class Identifier) - PXEClient
            var pxeVendor = "PXEClient";
            var pxeBytes = System.Text.Encoding.ASCII.GetBytes(pxeVendor);
            packet[249] = 60;  // Option 60
            packet[250] = (byte)pxeBytes.Length;
            for (int i = 0; i < pxeBytes.Length; i++)
                packet[251 + i] = pxeBytes[i];
            
            // End (255)
            packet[260] = 255;
            
            return packet;
        }

        private static void ParseDHCPResponse(byte[] data)
        {
            if (data.Length < 240)
            {
                Console.WriteLine($"[TEST] Response too short");
                return;
            }

            Console.WriteLine($"[TEST] --- DHCP Response ---");
            
            // Operation (1=Reply)
            Console.WriteLine($"[TEST] Op: {(data[0] == 2 ? "Reply" : "Request")}");
            
            // Transaction ID
            var xid = BitConverter.ToUInt32(data, 4);
            Console.WriteLine($"[TEST] Transaction ID: 0x{xid:X8}");
            
            // Your IP
            var yourIP = new IPAddress(data[16..20]);
            Console.WriteLine($"[TEST] Your IP: {yourIP}");
            
            // Server IP
            var serverIP = new IPAddress(data[20..24]);
            Console.WriteLine($"[TEST] Server IP: {serverIP}");
            
            // Boot file name (128-239)
            var bootFile = Encoding.ASCII.GetString(data, 128, 128).Trim('\0');
            if (!string.IsNullOrEmpty(bootFile))
                Console.WriteLine($"[TEST] Boot File: {bootFile}");
            
            // Options parsing
            Console.WriteLine($"[TEST] Options:");
            int i = 240;
            while (i < data.Length && data[i] != 255)
            {
                if (i + 1 >= data.Length) break;
                
                var option = data[i];
                var len = data[i + 1];
                
                if (option == 53) // DHCP Message Type
                {
                    var msgType = data[i + 2];
                    var typeStr = msgType switch
                    {
                        1 => "Discover",
                        2 => "Offer",
                        3 => "Request",
                        4 => "Decline",
                        5 => "ACK",
                        6 => "NAK",
                        7 => "Release",
                        8 => "Inform",
                        _ => "Unknown"
                    };
                    Console.WriteLine($"[TEST]   Option 53 (DHCP Message): {(DHCPMessageType)msgType} ({typeStr})");
                }
                else if (option == 54) // Server Identifier
                {
                    var serverID = new IPAddress(new byte[] { data[i+2], data[i+3], data[i+4], data[i+5] });
                    Console.WriteLine($"[TEST]   Option 54 (Server ID): {serverID}");
                }
                else if (option == 51) // Lease Time
                {
                    var lease = BitConverter.ToUInt32(data, i + 2);
                    Console.WriteLine($"[TEST]   Option 51 (Lease Time): {lease}s");
                }
                
                i += 2 + len;
            }
            
            Console.WriteLine($"[TEST] --- End Response ---");
        }
    }

    public enum DHCPMessageType : byte
    {
        Discover = 1,
        Offer = 2,
        Request = 3,
        Decline = 4,
        ACK = 5,
        NAK = 6,
        Release = 7,
        Inform = 8
    }
}