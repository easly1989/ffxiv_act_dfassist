using System;
using System.Speech.Synthesis;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class TTSHelper : IDisposable
    {
        private static TTSHelper _instance;
        public static TTSHelper Instance => _instance ?? (_instance = new TTSHelper());

        private MainControl _mainControl;
        private SpeechSynthesizer _synth;
        private ILocalizationRepository _localizationRepository;

        public TTSHelper()
        {
            _mainControl = Locator.Current.GetService<MainControl>();
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();

            _synth = new SpeechSynthesizer();
        }

        public void SendNotification(string message, string title = "ui-tts-dutyfound")
        {
            if(!_mainControl.TtsCheckBox.Checked)
                return;

            var dutyFound = _localizationRepository.GetText(title);
            _synth.SpeakAsync($"{dutyFound}; {message}");
        }

        public void Dispose()
        {
            _synth?.Dispose();

            _localizationRepository = null;
            _mainControl = null;
            _synth = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}
