using System.Runtime.InteropServices;

namespace DFAssist
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TcpTable
    {
        public uint length;
        public TcpRow row;
    }
}