using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace DFAssist.Helpers
{
    public class PatternScanner
    {
        private Process target;

        private readonly ILogger _logger;
        private MemHelper _memhelper;
        private uint SizeOfCode;
        private uint CodeBase;
        private uint _dataLength;
        private byte[] _data;
        private IntPtr _baseAddress;
        #region structs



        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DOS_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] e_magic;       // Magic number
            public UInt16 e_cblp;    // Bytes on last page of file
            public UInt16 e_cp;      // Pages in file
            public UInt16 e_crlc;    // Relocations
            public UInt16 e_cparhdr;     // Size of header in paragraphs
            public UInt16 e_minalloc;    // Minimum extra paragraphs needed
            public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
            public UInt16 e_ss;      // Initial (relative) SS value
            public UInt16 e_sp;      // Initial SP value
            public UInt16 e_csum;    // Checksum
            public UInt16 e_ip;      // Initial IP value
            public UInt16 e_cs;      // Initial (relative) CS value
            public UInt16 e_lfarlc;      // File address of relocation table
            public UInt16 e_ovno;    // Overlay number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt16[] e_res1;    // Reserved words
            public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo;     // OEM information; e_oemid specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public UInt16[] e_res2;    // Reserved words
            public Int32 e_lfanew;      // File address of new exe header

            private string _e_magic
            {
                get { return new string(e_magic); }
            }

            public bool isValid
            {
                get { return _e_magic == "MZ"; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }


        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_NT_HEADERS64
        {
            [FieldOffset(0)]
            public uint Signature;

            [FieldOffset(4)]
            public IMAGE_FILE_HEADER FileHeader;

            [FieldOffset(24)]
            public IMAGE_OPTIONAL_HEADER64 OptionalHeader;

            public bool isValid
            {
                get { return Signature == 0x00004550 && OptionalHeader.Magic == MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC; }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            [FieldOffset(0)]
            public MagicType Magic;

            [FieldOffset(2)]
            public byte MajorLinkerVersion;

            [FieldOffset(3)]
            public byte MinorLinkerVersion;

            [FieldOffset(4)]
            public uint SizeOfCode;

            [FieldOffset(8)]
            public uint SizeOfInitializedData;

            [FieldOffset(12)]
            public uint SizeOfUninitializedData;

            [FieldOffset(16)]
            public uint AddressOfEntryPoint;

            [FieldOffset(20)]
            public uint BaseOfCode;

            [FieldOffset(24)]
            public ulong ImageBase;

            [FieldOffset(32)]
            public uint SectionAlignment;

            [FieldOffset(36)]
            public uint FileAlignment;

            [FieldOffset(40)]
            public ushort MajorOperatingSystemVersion;

            [FieldOffset(42)]
            public ushort MinorOperatingSystemVersion;

            [FieldOffset(44)]
            public ushort MajorImageVersion;

            [FieldOffset(46)]
            public ushort MinorImageVersion;

            [FieldOffset(48)]
            public ushort MajorSubsystemVersion;

            [FieldOffset(50)]
            public ushort MinorSubsystemVersion;

            [FieldOffset(52)]
            public uint Win32VersionValue;

            [FieldOffset(56)]
            public uint SizeOfImage;

            [FieldOffset(60)]
            public uint SizeOfHeaders;

            [FieldOffset(64)]
            public uint CheckSum;

            [FieldOffset(68)]
            public SubSystemType Subsystem;

            [FieldOffset(70)]
            public DllCharacteristicsType DllCharacteristics;

            [FieldOffset(72)]
            public ulong SizeOfStackReserve;

            [FieldOffset(80)]
            public ulong SizeOfStackCommit;

            [FieldOffset(88)]
            public ulong SizeOfHeapReserve;

            [FieldOffset(96)]
            public ulong SizeOfHeapCommit;

            [FieldOffset(104)]
            public uint LoaderFlags;

            [FieldOffset(108)]
            public uint NumberOfRvaAndSizes;

            [FieldOffset(112)]
            public IMAGE_DATA_DIRECTORY ExportTable;

            [FieldOffset(120)]
            public IMAGE_DATA_DIRECTORY ImportTable;

            [FieldOffset(128)]
            public IMAGE_DATA_DIRECTORY ResourceTable;

            [FieldOffset(136)]
            public IMAGE_DATA_DIRECTORY ExceptionTable;

            [FieldOffset(144)]
            public IMAGE_DATA_DIRECTORY CertificateTable;

            [FieldOffset(152)]
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;

            [FieldOffset(160)]
            public IMAGE_DATA_DIRECTORY Debug;

            [FieldOffset(168)]
            public IMAGE_DATA_DIRECTORY Architecture;

            [FieldOffset(176)]
            public IMAGE_DATA_DIRECTORY GlobalPtr;

            [FieldOffset(184)]
            public IMAGE_DATA_DIRECTORY TLSTable;

            [FieldOffset(192)]
            public IMAGE_DATA_DIRECTORY LoadConfigTable;

            [FieldOffset(200)]
            public IMAGE_DATA_DIRECTORY BoundImport;

            [FieldOffset(208)]
            public IMAGE_DATA_DIRECTORY IAT;

            [FieldOffset(216)]
            public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

            [FieldOffset(224)]
            public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

            [FieldOffset(232)]
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        public enum MachineType : ushort
        {
            Native = 0,
            I386 = 0x014c,
            Itanium = 0x0200,
            x64 = 0x8664
        }
        public enum MagicType : ushort
        {
            IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
            IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
        }
        public enum SubSystemType : ushort
        {
            IMAGE_SUBSYSTEM_UNKNOWN = 0,
            IMAGE_SUBSYSTEM_NATIVE = 1,
            IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
            IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
            IMAGE_SUBSYSTEM_POSIX_CUI = 7,
            IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
            IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
            IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
            IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
            IMAGE_SUBSYSTEM_EFI_ROM = 13,
            IMAGE_SUBSYSTEM_XBOX = 14

        }
        public enum DllCharacteristicsType : ushort
        {
            RES_0 = 0x0001,
            RES_1 = 0x0002,
            RES_2 = 0x0004,
            RES_3 = 0x0008,
            IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,
            IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
            IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,
            IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
            IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
            IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
            RES_4 = 0x1000,
            IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
            IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
        }

        #endregion

        public PatternScanner(MemHelper memhelper)
        {
            _logger = Locator.Current.GetService<ILogger>();

            _memhelper = memhelper;

            _baseAddress = memhelper.BaseAddress;

            var dosHeaders = memhelper.Read<IMAGE_DOS_HEADER>(_baseAddress);
            _logger.Write($"dosHeaders.isvalid{dosHeaders.isValid}", LogLevel.Info);
            if (dosHeaders.isValid)
            {
                var ntHeaders = memhelper.Read<IMAGE_NT_HEADERS64>(_baseAddress + dosHeaders.e_lfanew);
                SizeOfCode = ntHeaders.OptionalHeader.SizeOfCode;
                CodeBase = ntHeaders.OptionalHeader.BaseOfCode;
                _dataLength = CodeBase + SizeOfCode;
                _data = memhelper.ReadBytes(_baseAddress, (int)_dataLength);
                _logger.Write($"SizeofCode:{SizeOfCode} CodeBase:{CodeBase}", LogLevel.Info);
            }

        }

        public IntPtr FindFunctionCall(IntPtr functionAddress)
        {
            bool funcTest(int start)
            {
                if (_data[start] != 0xE9) return false;

                var val = BitConverter.ToUInt32(_data, start + 1) + 5;
                var address = (long) _baseAddress + start + val;

                return (IntPtr) address == functionAddress;
            }

            uint plen = (uint)5;
            var dataLength = _dataLength - plen;
            for (var i = CodeBase; i < dataLength; i++)
            {
                if (funcTest((int)i))
                    return _baseAddress + (int)i;
            }

            return IntPtr.Zero;
        }



        public T FindSingle<T>(string pattern, Func<IntPtr, T> visitor)
        {
            var results = FindPattern(pattern);
            if (results.Count > 1)
                throw new ArgumentException($"Provided pattern found {results.Count}, only a single result is acceptable");

            return visitor(results[0]);
        }

        public IntPtr FindSingle(string pattern)
        {
            var results = FindPattern(pattern);
            if (results.Count > 1)
                throw new ArgumentException($"Provided pattern found {results.Count}, only a single result is acceptable");

            return results[0];
        }

        public List<IntPtr> FindPattern(string pattern)
        {
            var results = Find(HexToBytes(pattern));
            for (int i = 0; i < results.Count; i++)
            {
                results[i] = _baseAddress + (int)results[i];

            }
            return results;
        }

        List<IntPtr> Find(List<int> pattern)
        {

            List<IntPtr> ret = new List<IntPtr>();
            uint plen = (uint)pattern.Count;
            var dataLength = _dataLength - plen;
            for (var i = CodeBase; i < dataLength; i++)
            {
                if (ByteMatch(_data, (int)i, pattern))
                    ret.Add((IntPtr)i);
            }
            return ret;
        }

        bool ByteMatch(byte[] bytes, int start, List<int> pattern)
        {
            for (int i = start, j = 0; j < pattern.Count; i++, j++)
            {
                if (pattern[j] == -1)
                    continue;

                if (bytes[i] != pattern[j])
                    return false;
            }
            return true;
        }

        List<int> HexToBytes(string hex)
        {
            List<int> bytes = new List<int>();

            for (int i = 0; i < hex.Length - 1;)
            {
                if (hex[i] == '?')
                {
                    if (hex[i + 1] == '?')
                        i++;
                    i++;
                    bytes.Add(-1);
                    continue;
                }
                if (hex[i] == ' ')
                {
                    i++;
                    continue;
                }

                string byteString = hex.Substring(i, 2);
                var _byte = byte.Parse(byteString, NumberStyles.AllowHexSpecifier);
                bytes.Add(_byte);
                i += 2;
            }

            return bytes;
        }
    }
}
