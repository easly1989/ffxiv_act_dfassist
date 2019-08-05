using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Splat;

namespace DFAssist.Core.Network
{
    public struct IpPacket
    {
        public byte HeaderLength { get; }
        public ProtocolType Protocol { get; }
        public ProtocolFamily Version { get; }
        public IPAddress SourceIpAddress { get; }
        public IPAddress DestinationIpAddress { get; }

        public byte[] Data { get; }

        public bool IsValid { get; }

        public IpPacket(byte[] buffer)
        {
            try
            {
                var versionAndHeaderLength = buffer[0];
                Version = versionAndHeaderLength >> 4 == 4 ? ProtocolFamily.InterNetwork : ProtocolFamily.InterNetworkV6;
                HeaderLength = (byte)((versionAndHeaderLength & 15) * 4); // 0b1111 = 15

                Protocol = (ProtocolType)buffer[9];

                SourceIpAddress = new IPAddress(BitConverter.ToUInt32(buffer, 12));
                DestinationIpAddress = new IPAddress(BitConverter.ToUInt32(buffer, 16));

                Data = buffer.Skip(HeaderLength).ToArray();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Version = ProtocolFamily.Unknown;
                HeaderLength = 0;

                Protocol = ProtocolType.Unknown;

                SourceIpAddress = null;
                DestinationIpAddress = null;

                Data = null;

                IsValid = false;
                Locator.Current.GetService<ILogger>().Write(ex, "N: IP Packet Parsing Error", LogLevel.Error);
            }
        }
    }
}
