using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SaintCoinach;
using SaintCoinach.IO;
using SaintCoinach.Ex;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaintCoinach.Text;
using File = SaintCoinach.IO.File;

namespace DFDumper
{

    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": return "";
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
    class Program
    {
        
        static Regex removeHtmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        static void Main(string[] args)
        {

            var mapping = new Dictionary<Language, string>()
            {
                {Language.English,"en-us.json"},
                {Language.German,"de-de.json"},
                {Language.French,"fr-fr.json"},
                {Language.Japanese,"ja-jp.json"},
                //{Language.Korean,"ko-kr.json"},
            };

             
            foreach (var kvp in mapping)
            {
                var realm = new ARealmReversed(@"game-path", @"SaintCoinach.History.zip", kvp.Key, @"app_data.sqlite");
                realm.Packs.GetPack(new PackIdentifier("exd", PackIdentifier.DefaultExpansion, 0)).KeepInMemory = true;


                var cfcSheet = realm.GameData.GetSheet("ContentFinderCondition");

                JObject instances = new JObject();


                for (int i = 0; i < cfcSheet.Count; i++)
                {

                    var index = cfcSheet[i].Key.ToString();
                    var contents = cfcSheet[i, "Name"].ToString();

                    var name = removeHtmlRegex.Replace(contents, String.Empty).FirstCharToUpper();

                    if (!string.IsNullOrEmpty(name))
                    {

                        var item = new JProperty(
                            new JProperty(index, new JObject(
                                new JProperty("name", name))));
                        instances.Add(item);
                    }
                    
                }
                
                var rouletteSheet = realm.GameData.GetSheet("ContentRoulette");
                JObject roulettes = new JObject();
                for (int i = 0; i < rouletteSheet.Count; i++)
                {

                    var index = rouletteSheet[i].Key.ToString();
                    var contents = rouletteSheet[i, "Name"].ToString();

                    var name = removeHtmlRegex.Replace(contents, String.Empty).FirstCharToUpper();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var item = new JProperty(index, name);
                        roulettes.Add(item);
                    }
                       
                }


                JObject rss = new JObject(
                        new JProperty("version", $"{DateTime.Now:yyyymmdd}.1"),
                        new JProperty("instances", instances),
                        new JProperty("roulettes", roulettes)
                        );
               // new JArray(
               //                 from p in thing.ToArray()
               //                 orderby p.Key
               //                 select new JObject(new JProperty(p.Key.ToString(), new JObject(
               //                     new JProperty("name", p.DefaultValue.ToString())))))));


                System.IO.File.WriteAllText(kvp.Value, rss.ToString());
                GC.KeepAlive(mapping);

            }



            
        }
    }
}
