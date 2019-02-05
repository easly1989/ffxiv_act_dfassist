namespace DFAssist
{
    public class ProcessNet
    {
        public Network Network { get; }
        public System.Diagnostics.Process Process { get; }

        public ProcessNet(System.Diagnostics.Process process, Network network)
        {
            Process = process;
            Network = network;
        }
    }
}