using System.Diagnostics;

namespace DFAssist.Core.Network
{
    public class ProcessNetwork
    {
        public Network Network { get; }
        public Process Process { get; }

        public ProcessNetwork(Process process, Network network)
        {
            Process = process;
            Network = network;
        }
    }
}