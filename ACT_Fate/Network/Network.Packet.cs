using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace App
{
    internal partial class Network
    {
        private struct IPPacket
        {
            public ProtocolFamily Version;
            public byte HeaderLength;
            public ProtocolType Protocol;

            public IPAddress SourceIPAddress;
            public IPAddress DestinationIPAddress;

            public byte[] Data;

            public bool IsValid;

            public IPPacket(byte[] buffer)
            {
                try
                {
                    var versionAndHeaderLength = buffer[0];
                    Version = versionAndHeaderLength >> 4 == 4 ? ProtocolFamily.InterNetwork : ProtocolFamily.InterNetworkV6;
                    HeaderLength = (byte)((versionAndHeaderLength & 15) * 4); // 0b1111 = 15
                    
                    Protocol = (ProtocolType)buffer[9];

                    SourceIPAddress = new IPAddress(BitConverter.ToUInt32(buffer, 12));
                    DestinationIPAddress = new IPAddress(BitConverter.ToUInt32(buffer, 16));
                    
                    Data = buffer.Skip(HeaderLength).ToArray();

                    IsValid = true;
                }
                catch (Exception ex)
                {
                    Version = ProtocolFamily.Unknown;
                    HeaderLength = 0;

                    Protocol = ProtocolType.Unknown;

                    SourceIPAddress = null;
                    DestinationIPAddress = null;

                    Data = null;

                    IsValid = false;
                    Log.Ex(ex, "l-packet-error-ip");
                }
            }
        }

        private struct TCPPacket
        {
            public ushort SourcePort;
            public ushort DestinationPort;
            public byte DataOffset;
            public TCPFlags Flags;

            public byte[] Payload;

            public bool IsValid;

            public TCPPacket(byte[] buffer)
            {
                try
                {
                    SourcePort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
                    DestinationPort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 2));

                    var offsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 12));
                    DataOffset = (byte)((offsetAndFlags >> 12) * 4);
                    Flags = (TCPFlags)(offsetAndFlags & 511); // 0b111111111 = 511

                    Payload = buffer.Skip(DataOffset).ToArray();

                    IsValid = true;
                }
                catch (Exception ex)
                {
                    SourcePort = 0;
                    DestinationPort = 0;
                    DataOffset = 0;
                    Flags = TCPFlags.NONE;

                    Payload = null;

                    IsValid = false;

                    Log.Ex(ex, "l-packet-error-tcp");
                }
            }
        }

        [Flags]
        public enum TCPFlags
        {
            NONE = 0,
            FIN = 1,
            SYN = 2,
            RST = 4,
            PSH = 8,
            ACK = 16,
            URG = 32,
            ECE = 64,
            CWR = 128,
            NS = 256,
        }
    }
}
