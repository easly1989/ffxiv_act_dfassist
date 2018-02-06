using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace DFAssist
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TcpRow
    {
        public TcpState state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public uint owningPid;
    }
}