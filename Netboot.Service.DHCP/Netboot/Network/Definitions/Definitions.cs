using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Network.Definitions
{
	public enum DHCPHardwareType : byte
	{
		Ethernet = 1,
		IEEE802 = 6,
		ARCNet = 7,
		LocalTalk = 11,
		LocalNet = 12,
		SMDS = 14,
		FrameRelay = 15,
		ATM1 = 16,
		HDLC = 17,
		FireChannel = 18,
		ATM2 = 19,
		SerialLine = 20
	}

	public enum BOOTPVendor : uint
	{
		/// <summary>
		///	The BOOTP Packet has no Vendor specific options set.
		/// </summary>
		DHCP = 1666417251,
	}

	public enum PXEVendorID : byte
	{
		None,
		PXEClient,
		PXEServer,
		AAPLBSDPC,
		Msft,
	}

	public enum DHCPMessageType
	{
		None = 0,
		/// <summary>
		/// Sent by the client as the first step of the DHCP client/server interaction.
		/// The purpose of the DHCPDISCOVER is for the client to "discover" what servers are out there and what network parameters they have to offer.
		/// </summary>
		Discover = 1,
		/// <summary>
		/// Sent by the server to the client in response to a DHCPDISCOVER. The server uses the DHCPOFFER message to "offer" an IP address,
		/// lease time, and network configuration parameters to the client.
		/// </summary>
		Offer = 2,
		/// <summary>
		/// Sent by the client to the server in response to a DHCPOFFER. The "server identifier"
		/// field of the DHCPREQUEST indicates which server the client has chosen to further interact with.
		/// All servers that sent the client a DHCPOFFER receive the DHCPREQUEST. The ones that are not chosen simply use the message as notification
		/// that they have not been chosen. The server that is chosen responds to the request, either with a DHCPACK or a DHCPNAK.
		/// </summary>
		Request = 3,
		/// <summary>
		/// Sent by the server to the client in response to a DHCPREQUEST. The DHCPACK indicates that the server "acknowledges" the request,
		/// and the DHCPACK message contains fields which indicate the IP address, lease time,
		/// and network configuration parameters that the client is being configured with.
		/// </summary>
		Ack = 4,
		/// <summary>
		/// Sent by the server to the client in response to a DHCPREQUEST. The DHCPNAK indicates that the server does not acknowledge the request,
		/// and does not agree to lease the specified IP address.
		/// </summary>
		Nak = 5,
		/// <summary>
		/// Sent by the client to the server to give up an IP address lease. If the client knows that it no longer needs an IP address,
		/// it should send the server a DHCPRELEASE.
		/// </summary>
		Release = 6,
		/// <summary>
		/// Sent by the client to the server in response to a DHCPACK. If the client receives a DHCPACK, but, for some reason,
		/// is not satisfied with the lease time and/or network parameters in the message,it can send the server a DHCPDECLINE
		/// indicating that it refuses to use the IP address. 
		/// </summary>
		Decline = 7
	}

	public enum BOOTPOPCode : byte
	{
		BootRequest = 1,
		BootReply = 2
	}
}
