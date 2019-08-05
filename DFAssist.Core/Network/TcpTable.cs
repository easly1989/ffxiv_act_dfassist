using System.Runtime.InteropServices;

namespace DFAssist.Core.Network
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TcpTable
    {
        public uint length;
        public TcpRow row;
    }
}