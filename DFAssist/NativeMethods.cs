using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DFAssist
{
    public static class NativeMethods
    {
        [DllImport("Iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedTcpTable(IntPtr tcpTable, ref int tcpTableLength, bool sort, AddressFamily ipVersion, int tcpTableType, int reserved);
    }
}