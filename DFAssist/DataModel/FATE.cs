namespace DFAssist.DataModel
{
    public class Fate
    {
        public Area Area { get; set; }
        public string Name { get; set; }

        public static explicit operator Fate(string name)
        {
            return new Fate { Name = name };
        }
    }
}
