namespace DFAssist.DataModel
{
    public class Roulette
    {
        public string Name { get; set; }

        public static explicit operator Roulette(string name)
        {
            return new Roulette { Name = name };
        }
    }
}
