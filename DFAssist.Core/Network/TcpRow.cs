using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace DFAssist.Core.Network
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