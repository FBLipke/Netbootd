﻿using Netboot.Network.Interfaces;
using Netboot.Network.Packet;
using System.Net;

namespace Netboot.Network.EventHandler
{
    public class DataReceivedEventArgs
    {
        public byte[] Packet { get; private set; }
        public IPEndPoint RemoteEndpoint { get; private set; }
        public Guid SocketId { get; private set; }
        public Guid ServerId { get; private set; }
        public string ServiceType { get; private set; }

        public DataReceivedEventArgs(string serviceType, Guid serverId,
            Guid socketId, byte[] packet, IPEndPoint remoteEndpoint)
        {
            ServiceType = serviceType;
            ServerId = serverId;
            SocketId = socketId;
            Packet = packet;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}