using System;

namespace DFAssist.Core.Network
{
    [Flags]
    public enum TcpFlags
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
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
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
    }
}