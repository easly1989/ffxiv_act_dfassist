using System.Speech.Synthesis;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class TTSHelper : BaseNotificationHelper<TTSHelper>
    {
        private SpeechSynthesizer _synth;
        private ILocalizationRepository _localizationRepository;

        public TTSHelper()
        {
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();

            _synth = new SpeechSynthesizer();
        }

        protected override void OnSendNotification(string title, string message, string testing)
        {
            if(!MainControl.TtsCheckBox.Checked)
            {
                Logger.Write("UI: TTS is disabled!", LogLevel.Debug);
                return;
            }

            Logger.Write("UI: Sending TTS Notification...", LogLevel.Debug);
            _synth.SpeakAsync($"{title}; {message}");
            Logger.Write("UI: TTS notification sent!", LogLevel.Debug);
        }

        protected override void OnDisposeOwnedObjects()
        {
            _synth?.Dispose();

            base.OnDisposeOwnedObjects();
        }

        protected override void OnSetNullOwnedObjects()
        {
            _localizationRepository = null;
            _synth = null;

            base.OnSetNullOwnedObjects();
        }
    }
    // ReSharper restore InconsistentNaming
}
