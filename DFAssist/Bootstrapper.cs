using DFAssist.Contracts;
using DFAssist.Contracts.Repositories;
using DFAssist.Core.Network;
using DFAssist.Core.Repositories;
using Splat;

namespace DFAssist
{
    public class Bootstrapper : IEnableLogger
    {
        public Bootstrapper()
        {
            Register();
        }

        public void Register()
        {
            Locator.CurrentMutable.RegisterConstant(new Logger(), typeof(ILogger));
            Locator.CurrentMutable.RegisterConstant(new LocalizationRepository(), typeof(ILocalizationRepository));
            Locator.CurrentMutable.RegisterConstant(new DataRepository(), typeof(IDataRepository));
            Locator.CurrentMutable.RegisterConstant(new FFXIVPacketHandler(), typeof(IPacketHandler));
        }
    }
}
