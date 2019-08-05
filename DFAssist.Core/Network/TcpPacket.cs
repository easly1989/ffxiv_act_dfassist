using System;
using System.Linq;
using System.Net;
using Splat;

namespace DFAssist.Core.Network
{
    public struct TcpPacket
    {
        public ushort SourcePort;
        public ushort DestinationPort;
        public byte DataOffset;
        public TcpFlags Flags;

        public byte[] Payload;

        public bool IsValid;

        public TcpPacket(byte[] buffer)
        {
            try
            {
                SourcePort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
                DestinationPort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 2));

                var offsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 12));
                DataOffset = (byte)((offsetAndFlags >> 12) * 4);
                Flags = (TcpFlags)(offsetAndFlags & 511); // 0b111111111 = 511

                Payload = buffer.Skip(DataOffset).ToArray();

                IsValid = true;
            }
            catch (Exception ex)
            {
                SourcePort = 0;
                DestinationPort = 0;
                DataOffset = 0;
                Flags = TcpFlags.NONE;

                Payload = null;
                IsValid = false;

                Locator.Current.GetService<ILogger>().Write(ex, "N: TCP Packet Parsing Error", LogLevel.Error);
            }
        }
    }
}