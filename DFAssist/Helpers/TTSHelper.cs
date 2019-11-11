using System.Linq;
using System.Speech.Synthesis;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class TTSHelper : BaseNotificationHelper<TTSHelper>
    {
        private SpeechSynthesizer _synth;

        public InstalledVoice[] AvailableVoices => _synth != null
                                                   ? _synth.GetInstalledVoices().Where(x => x.Enabled).ToArray()
                                                   : new InstalledVoice[] {};

        public TTSHelper()
        {
            _synth = new SpeechSynthesizer();
        }

        public void SelectVoice(string voiceName)
        {
            _synth?.SelectVoice(voiceName);
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
            _synth = null;

            base.OnSetNullOwnedObjects();
        }
    }
    // ReSharper restore InconsistentNaming
}
