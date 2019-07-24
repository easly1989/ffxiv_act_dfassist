using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DFAssist
{
    public static class NativeMethods
    {
        [DllImport("user32")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        public static extern bool AnimateWindow(IntPtr hWnd, int dwTime, int dwFlags);

        [DllImport("User32.DLL")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedTcpTable(
            IntPtr tcpTable,
            ref int tcpTableLength,
            bool sort,
            AddressFamily ipVersion,
            int tcpTableType,
            int reserved);
    }
}