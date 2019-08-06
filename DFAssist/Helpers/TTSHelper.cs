using System;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class TTSHelper : IDisposable
    {
        private static TTSHelper _instance;
        public static TTSHelper Instance => _instance ?? (_instance = new TTSHelper());

        public void Dispose()
        {
        }
    }
    // ReSharper restore InconsistentNaming
}
