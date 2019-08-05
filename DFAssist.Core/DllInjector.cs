using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DFAssist.Core
{
    public enum DllInjectionResult
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
        DllNotFound,
        ProcessNotFound,
        InjectionFailed,
        InjectionSuccess,
        EjectionFailed,
        EjectionSuccess
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
    }

    public sealed class DllInjector
    {
        // ReSharper disable ArrangeTypeMemberModifiers
        // ReSharper disable InconsistentNaming
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;
        // ReSharper restore InconsistentNaming

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        // ReSharper restore ArrangeTypeMemberModifiers

        private static DllInjector _instance;
        public static DllInjector GetInstance => _instance ?? (_instance = new DllInjector());

        private DllInjector()
        {
        }

        public DllInjectionResult Inject(string sProcName, string sDllPath)
        {
            if (!File.Exists(sDllPath))
            {
                return DllInjectionResult.DllNotFound;
            }

            var procs = Process.GetProcesses();
            var procId = (from t in procs where t.ProcessName == sProcName select (uint)t.Id).FirstOrDefault();

            if (procId == 0)
                return DllInjectionResult.ProcessNotFound;

            return !InternalInject(procId, sDllPath)
                ? DllInjectionResult.InjectionFailed
                : DllInjectionResult.InjectionSuccess;
        }

        private static bool InternalInject(uint processId, string dllPath)
        {
            var hndProc = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, processId);
            if (hndProc == INTPTR_ZERO)
                return false;

            var lpLlAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (lpLlAddress == INTPTR_ZERO)
                return false;

            var lpAddress = VirtualAllocEx(hndProc, (IntPtr)null, (IntPtr)dllPath.Length, (0x1000 | 0x2000), 0X40);
            if (lpAddress == INTPTR_ZERO)
                return false;

            var bytes = Encoding.ASCII.GetBytes(dllPath);
            if (WriteProcessMemory(hndProc, lpAddress, bytes, (uint)bytes.Length, 0) == 0)
                return false;

            if (CreateRemoteThread(hndProc, (IntPtr)null, INTPTR_ZERO, lpLlAddress, lpAddress, 0, (IntPtr)null) == INTPTR_ZERO)
                return false;

            CloseHandle(hndProc);
            return true;
        }
    }
}