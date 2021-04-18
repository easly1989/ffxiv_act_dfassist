using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace DFAssist.Helpers
{
    public class OpCodeHelper
    {
        private readonly ILogger _logger;
        private PatternScanner patternscanner;
        private MemHelper memhelper;
        public OpCodeHelper()
        {
            _logger = Locator.Current.GetService<ILogger>();
            var target = FFXIVNetworkProcessHelper.Instance.ActiveProcess;
            memhelper = new MemHelper(target);
            patternscanner = new PatternScanner(memhelper);
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        public ushort GetAlertOpCode()
        {
            var jumpTableIndexOffset = patternscanner.FindSingle("8D 42 ? 3D ? ? ? ? 0f 87 ? ? ? ? 4c", ptr =>
            {
                byte value = memhelper.Read<byte>(ptr + 0x2);
                _logger.Write($"raw offset {value}", LogLevel.Warn);

                return (value ^ 0x80) - 0x80;
            });

            _logger.Write($"Jumptable index offset 0x{(int)jumpTableIndexOffset:X}", LogLevel.Warn);

            var functionptr = patternscanner.FindSingle("48 89 5C 24 ? 57 48 83 EC 20 80 79 55 00");
            var functionRef = patternscanner.FindFunctionCall(functionptr);

            _logger.Write($"Found function call of 0x{(ulong)functionptr:X} at 0x{(ulong)functionRef:X}",LogLevel.Warn);





            //Read backwards until 0xCC CC is found to get the start of the queue function
            var memory = memhelper.ReadBytes(functionRef - 0x200, 0x205);
            //_logger.Write("read bytes",LogLevel.Warn);
            Array.Reverse(memory);
            //_logger.Write(ByteArrayToString(memory),LogLevel.Warn);
            functionptr = IntPtr.Zero;
            for (int i = 0; i < memory.Length; i++)
            {
                //if (memory[i] == 0xCC && memory[i - 1] == 0xCC && memory[i - 2] == 0xCC && memory[i - 3] == 0xCC)
                var m = BitConverter.ToUInt32(memory, i);
                //_logger.Write($"Fo 0x{m:X} {i}", LogLevel.Warn);
                if (m == 0xCCCCCCCC)
                {
                    functionptr = functionRef - i + 5;
                    _logger.Write($"functionRef:0x{(ulong)functionRef:X} functionptr:0x{(ulong)functionptr:X} {i}", LogLevel.Warn);
                    break;
                }
            }


            //_logger.Write("Finished loop", LogLevel.Warn);
            functionRef = patternscanner.FindFunctionCall(functionptr);
            _logger.Write($"Found function call of 0x{(ulong)functionptr:X} at 0x{(ulong)functionRef:X}", LogLevel.Warn);




            //Go back 0x13 bytes from the function call location to get where the jumptable points to
            var jumplocation = functionRef - 0x13;

            var jumptablevalues =
                patternscanner.FindSingle("41 8B 8C 80 ? ? ? ? 49 03 C8 FF E1 B9 ? ? ? ? ? ? ? ? ? ? 48", ptr =>
                {
                    var jumptableAddress = memhelper.BaseAddress + memhelper.Read<int>(ptr + 4);
                    _logger.Write($"Found jumptable of 0x{(ulong)jumptableAddress:X} at 0x{(ulong)ptr:X}", LogLevel.Warn);

                    //Just read a large number of values, its ~900 ingame so set a larger static value so we don't need another pattern
                    return memhelper.Read<int>(jumptableAddress, 1024);
                });


            for (int i = 0; i < jumptablevalues.Length; i++)
            {

                //_logger.Write($"{(ulong)(memhelper.BaseAddress + jumptablevalues[i]):X}", LogLevel.Warn);
                if (memhelper.BaseAddress + jumptablevalues[i] == jumplocation)
                {

                    if (jumpTableIndexOffset < 0)
                    { 
                        return (ushort)(i - jumpTableIndexOffset);
                    }

                    return (ushort)(i + jumpTableIndexOffset);

                }
            }
            _logger.Write($"Failed to find alert opcode", LogLevel.Warn);
            throw new Exception("Failed to find alert opcode");
        }
    }
}
