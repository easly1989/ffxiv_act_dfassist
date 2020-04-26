using System;
using DFAssist.Contracts.Duty;

namespace DFAssist.Contracts
{
    public interface IPacketHandler
    {
        void UnregisterHandlers();
        void UnRegisterMessageHandler(ushort opcode);
        void RegisterMessageHandler(ushort opcode, Action<byte[]> eventhandler);
        void HandleMessage(byte[] message);
    }
}
