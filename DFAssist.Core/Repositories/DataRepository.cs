using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DFAssist.Contracts.DataModel;
using DFAssist.Contracts.Repositories;
using Newtonsoft.Json;
using Splat;

namespace DFAssist.Core.Repositories
{
    public class DataRepository : RepositoryBase, IDataRepository
    {
        private Dictionary<int, Instance> _instances;
        private Dictionary<int, Roulette> _roulettes;

        public DataRepository()
        {
            _instances = new Dictionary<int, Instance>();
            _roulettes = new Dictionary<int, Roulette>();
        }

        public Instance GetInstance(int code)
        {
            if (_instances.TryGetValue(code, out var instance))
                return instance;

            Logger.Write($"Could not find Duty with {code}, report to the dev!", LogLevel.Warn);
            return new Instance { Name = $"Unknown Duty ({code})" };
        }

        public Roulette GetRoulette(int code)
        {
            if (_roulettes.TryGetValue(code, out var roulette))
                return roulette;

            Logger.Write($"Could not find Roulette with {code}, report to the dev!", LogLevel.Warn);
            return new Roulette { Name = $"Unknown Roulette ({code})" };
        }

        protected override void OnLocalUpdatedRequested(string pluginPath, string language)
        {
            Logger.Write($"Updating data for language {language}...", LogLevel.Debug);
            var result = ReadFromFile(Path.Combine(pluginPath, "data", $"{language}.json"));
            if (string.IsNullOrWhiteSpace(result))
            {
                Logger.Write($"Unable to read {language} file", LogLevel.Error);
                return;
            }

            InternalUpdate(result, language);
            Logger.Write(_instances.Any() && _roulettes.Any() ? $"Data {Version} Initialized!" : "Unable to initialize Data!", LogLevel.Debug);
        }

        protected override void OnWebUpdateRequested()
        {
            Logger.Write("Updating data files from web...", LogLevel.Debug);
            WebUpdateRoutine("data");
            Logger.Write("Data files update completed", LogLevel.Debug);
        }

        private void InternalUpdate(string json, string language)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                var data = JsonConvert.DeserializeObject<GameData>(json);
                var version = data.Version;

                if (version > Version || CurrentLanguage != language)
                {
                    _instances = data.Instances;
                    _roulettes = data.Roulettes;
                    Version = version;

                    Initialized = true;
                    CurrentLanguage = language;
                    Logger.Write($"UI: Data updated to v{version} for {language}", LogLevel.Info);
                }
                else
                {
                    Logger.Write("UI: Data already updated!", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "UI: An Error occurrend while updating data...", LogLevel.Error);
            }
        }
    }
}
