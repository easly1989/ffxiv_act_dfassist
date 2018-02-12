using System;
using System.Collections.Generic;
using DFAssist.DataModel;
using Newtonsoft.Json;

namespace DFAssist
{
    public static class Data
    {
        public static bool Initialized { get; private set; }
        public static decimal Version { get; private set; }
        public static string CurrentLanguage { get; private set; }

        public static IReadOnlyDictionary<int, Area> Areas { get; private set; } = new Dictionary<int, Area>();
        public static IReadOnlyDictionary<int, Instance> Instances { get; private set; } = new Dictionary<int, Instance>();
        public static IReadOnlyDictionary<int, Roulette> Roulettes { get; private set; } = new Dictionary<int, Roulette>();
        public static IReadOnlyDictionary<int, Fate> Fates { get; private set; } = new Dictionary<int, Fate>();

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
                    var fates = new Dictionary<int, Fate>();
                    foreach (var area in data.Areas)
                    {
                        foreach (var fate in area.Value.Fates)
                        {
                            fate.Value.Area = area.Value;
                            fates.Add(fate.Key, fate.Value);
                        }
                    }

                    Areas = data.Areas;
                    Instances = data.Instances;
                    Roulettes = data.Roulettes;
                    Fates = fates;
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
            if (Instances.TryGetValue(code, out var instance))
            {
                return instance;
            }

            return new Instance { Name = Localization.GetText("l-unknown-instance", code) };
        }

        public static Roulette GetRoulette(int code)
        {
            if (Roulettes.TryGetValue(code, out var roulette))
            {
                return roulette;
            }

            return new Roulette { Name = Localization.GetText("l-unknown-roulette", code) };
        }

        public static Area GetArea(int code)
        {
            if (Areas.TryGetValue(code, out var area))
            {
                return area;
            }

            return new Area { Name = Localization.GetText("l-unknown-area", code) };
        }

        public static Fate GetFate(int code)
        {
            if (Fates.ContainsKey(code))
            {
                return Fates[code];
            }

            return new Fate { Name = Localization.GetText("l-unknown-fate", code) };
        }
    }
}
