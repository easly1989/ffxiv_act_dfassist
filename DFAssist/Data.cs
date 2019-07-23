using System;
using System.Collections.Generic;
using System.Linq;
using DFAssist.DataModel;
using Newtonsoft.Json;

namespace DFAssist
{
    public static class Data
    {
        public static bool Initialized { get; private set; }
        public static decimal Version { get; private set; }
        public static string CurrentLanguage { get; private set; }

        public static IReadOnlyDictionary<int, Instance> Instances { get; private set; } = new Dictionary<int, Instance>();
        public static IReadOnlyDictionary<int, Roulette> Roulettes { get; private set; } = new Dictionary<int, Roulette>();
        
        public static void Initialize(string language)
        {
            var json = WebInteractions.DownloadString($"https://raw.githubusercontent.com/easly1989/ffxiv_act_dfassist/master/data/{language}.json");
            Fill(json, language);
        }

        private static void Fill(string json, string language)
        {
            if(string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                var data = JsonConvert.DeserializeObject<GameData>(json);
                var version = data.Version;

                if (version > Version || CurrentLanguage != language)
                {
                    Instances = data.Instances;
                    Roulettes = data.Roulettes;
                    Version = version;

                    if (Initialized)
                    {
                        Logger.Info( "l-data-updated", Version);
                    }

                    Initialized = true;
                    CurrentLanguage = language;
                }
                else
                {
                    Logger.Info("l-data-is-latest", Version);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "l-data-error");
            }
        }

        public static Instance GetInstance(int code)
        {
            return Instances.TryGetValue(code, out var instance) ? instance : new Instance { Name = Localization.GetText("l-unknown-instance", code) };
        }

        public static Roulette GetRoulette(int code)
        {
            return Roulettes.TryGetValue(code, out var roulette) ? roulette : new Roulette { Name = Localization.GetText("l-unknown-roulette", code) };
        }
    }
}
