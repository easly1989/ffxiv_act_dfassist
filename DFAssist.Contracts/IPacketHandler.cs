using System;
using DFAssist.Contracts.Duty;

namespace DFAssist.Contracts
{
    public interface IPacketHandler
    {
        void HandleMessage(byte[] message, Action<EventType, int[]> fireEvent);
    }
}
