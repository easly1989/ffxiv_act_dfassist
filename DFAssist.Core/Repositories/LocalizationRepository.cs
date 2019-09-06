using System;
using System.Collections.Generic;
using System.IO;
using DFAssist.Contracts.Repositories;
using Newtonsoft.Json;
using Splat;

namespace DFAssist.Core.Repositories
{
    public class LocalizationRepository : RepositoryBase, ILocalizationRepository
    {
        private Dictionary<string, string> _localizedMap;

        public LocalizationRepository()
        {
            _localizedMap = new Dictionary<string, string>();
        }

        public string GetText(string codeToTranslate, params object[] arguments)
        {
            return GetText(codeToTranslate, string.Empty, arguments);
        }

        public string GetText(string codeToTranslate, string fallBackMessage, params object[] arguments)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codeToTranslate))
                {
                    Logger.Write("The string was empty, this is a code flaw, contact the dev!", LogLevel.Warn);
                    return string.Empty;
                }

                if (_localizedMap.TryGetValue(codeToTranslate, out var value))
                    return string.Format(value, arguments);

                Logger.Write("Unable to find the string {codeToTranslate}.", LogLevel.Warn);
            }
            catch (Exception e)
            {
                Logger.Write(e, $"Possible too few arguments provided for text {codeToTranslate}, contact the dev!", LogLevel.Error);
            }

            // if we get here something went wrong, but we should at least get some string out of this ^^'
            return string.IsNullOrWhiteSpace(fallBackMessage) ? codeToTranslate : fallBackMessage;
        }

        protected override void OnLocalUpdatedRequested(string pluginPath, string language)
        {
            Logger.Write($"Changing language to {language}...", LogLevel.Debug);
            var result = ReadFromFile(Path.Combine(pluginPath, "localization", $"{language}.json"));
            if(string.IsNullOrWhiteSpace(result))
            {
                Logger.Write($"Unable to read {language} file", LogLevel.Error);
                _localizedMap = new Dictionary<string, string>();
                return;
            }

            _localizedMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            Initialized = true;
            CurrentLanguage = language;
            Logger.Write($"Language changed to {language}", LogLevel.Info);
        }

        protected override void OnWebUpdateRequested(string pluginPath)
        {
            Logger.Write("Updating Localization files from web...", LogLevel.Debug);
            WebUpdateRoutine(pluginPath, "localization");
            Logger.Write("Localization files update completed", LogLevel.Debug);
        }
    }
}
