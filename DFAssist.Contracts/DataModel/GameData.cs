using System.Collections.Generic;

namespace DFAssist.Contracts.DataModel
{
    public class GameData
    {
        public decimal Version { get; set; }
        public Dictionary<int, Instance> Instances { get; set; }
        public Dictionary<int, Roulette> Roulettes { get; set; }
    }
}
