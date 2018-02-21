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

        public static IReadOnlyDictionary<int, Area> Areas { get; private set; } = new Dictionary<int, Area>();
        public static IReadOnlyDictionary<int, Instance> Instances { get; private set; } = new Dictionary<int, Instance>();
        public static IReadOnlyDictionary<int, Roulette> Roulettes { get; private set; } = new Dictionary<int, Roulette>();
        public static IReadOnlyDictionary<int, Fate> Fates { get; private set; } = new Dictionary<int, Fate>();

        public static void Initialize(string language)
        {
            var json = WebInteractions.DownloadString($@"D:\GIT\ffxiv_act_dfassist\data\{language}.json");
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

        public static Instance GetInstance(int code, bool oldId = false)
        {
            if (oldId)
                return Instances.Values.FirstOrDefault(x => x.OldId == code);

            return Instances.TryGetValue(code, out var instance) ? instance : new Instance { Name = Localization.GetText("l-unknown-instance", code) };
        }

        public static Roulette GetRoulette(int code)
        {
            return Roulettes.TryGetValue(code, out var roulette) ? roulette : new Roulette { Name = Localization.GetText("l-unknown-roulette", code) };
        }

        public static Area GetArea(int code)
        {
            return Areas.TryGetValue(code, out var area) ? area : new Area { Name = Localization.GetText("l-unknown-area", code) };
        }

        public static Fate GetFate(int code)
        {
            return Fates.ContainsKey(code) ? Fates[code] : new Fate { Name = Localization.GetText("l-unknown-fate", code) };
        }
    }
}
