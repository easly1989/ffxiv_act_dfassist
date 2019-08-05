using DFAssist.Contracts.Duty;

namespace DFAssist.Contracts
{
    public delegate void PacketDelegate(int pid, EventType eventType, int[] args);

    public interface IPacketHandler
    {
        event PacketDelegate OnEventReceived;

        void Analyze(int processId, byte[] payload, ref MatchingState state);
    }
}
