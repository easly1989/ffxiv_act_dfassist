namespace DFAssist.Contracts.DataModel
{
    public class Roulette
    {
        public string Name { get; set; }

        // needed to deserialize from json!
        public static explicit operator Roulette(string name)
        {
            return new Roulette { Name = name };
        }
    }
}
