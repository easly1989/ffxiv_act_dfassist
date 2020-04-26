using System;
using System.Collections.Generic;
using System.Linq;
using DFAssist.Contracts;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Core.Network
{
    // ReSharper disable InconsistentNaming
    public class FFXIVPacketHandler : IPacketHandler
    // ReSharper restore InconsistentNaming
    {
        private readonly ILogger _logger;
        private readonly IDataRepository _dataRepository;

        public FFXIVPacketHandler()
        {
            _logger = Locator.Current.GetService<ILogger>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();

        }

        private readonly Dictionary<ushort,Action<byte[]>> _opCodeHandler = new Dictionary<ushort, Action<byte[]>>();

        public void UnregisterHandlers()
        {
            _opCodeHandler.Clear();
        }

        public void UnRegisterMessageHandler(ushort opcode)
        {
            _opCodeHandler.Remove(opcode);
        }

        public void RegisterMessageHandler(ushort opcode, Action<byte[]> eventhandler)
        {
            _opCodeHandler.Add(opcode,eventhandler);
        }


        public void HandleMessage(byte[] message)
        {
            try
            {
                if (message.Length < 32)
                {
                    // type == 0x0000 (Messages were filtered here)
                    return;
                }

                var opcode = BitConverter.ToUInt16(message, 18);


#if DEBUG
                _logger.Write($"--- Received opcode: {opcode}", LogLevel.Warn);
#endif
                var data = message.Skip(32).ToArray();

                if (_opCodeHandler.TryGetValue(opcode, out var handler))
                {
                    handler(data);
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.Write(ex, "A: Error while analyzing Message", LogLevel.Error);
            }
        }
    }
}
