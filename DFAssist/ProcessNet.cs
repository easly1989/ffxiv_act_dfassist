using System.Diagnostics;

namespace DFAssist
{
    public class ProcessNet
    {
        public Network Network { get; }
        public Process Process { get; }

        public ProcessNet(Process process, Network network)
        {
            Process = process;
            Network = network;
        }
    }
}