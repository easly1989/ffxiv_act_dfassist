using System.Collections.Generic;

namespace DFAssist.DataModel
{
    public class Area
    {
        public string Name { get; set; }
        public IReadOnlyDictionary<int, Fate> Fates { get; set; }
    }
}
