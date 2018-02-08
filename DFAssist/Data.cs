using System;
using System.Collections.Generic;
using System.IO;
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
            var json = File.ReadAllText(@"D:\GIT\ffxiv_act_dfassist\data\en-us.json"); //$"https://raw.githubusercontent.com/easly1989/ffxiv_act_dfassist/master/data/{language}.json";
            Fill(json, language);
        }

        private static void Fill(string json, string language)
        {
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
                        Logger.LogInfo( "l-data-updated", Version);
                    }

                    Initialized = true;
                    CurrentLanguage = language;
                }
                else
                {
                    Logger.LogInfo("l-data-is-latest", Version);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "l-data-error");
            }
        }

        public static Instance GetInstance(int code)
        {
            if (Instances.TryGetValue(code, out var instance))
            {
                return instance;
            }

            if (code != 0)
                Logger.LogError($"Missing instance code: {code}");

            return new Instance { Name = "<unknown-instance>" };
        }

        public static Roulette GetRoulette(int code)
        {
            if (Roulettes.TryGetValue(code, out var roulette))
            {
                return roulette;
            }

            if (code != 0)
                Logger.LogError($"Missing Roulette code: {code}");

            return new Roulette { Name = "<unknown-roulette>" };
        }

        public static Area GetArea(int code)
        {
            if (Areas.TryGetValue(code, out var area))
            {
                return area;
            }

            if (code != 0)
                Logger.LogError($"Missing area code: {code}");

            return new Area { Name = "<unknown-area>" };
        }

        public static Fate GetFate(int code)
        {
            if (Fates.ContainsKey(code))
            {
                return Fates[code];
            }

            if (code != 0)
                Logger.LogError($"Missing FATE code: {code}");

            return new Fate { Name = "<unknown-fate>" };
        }
    }
}
