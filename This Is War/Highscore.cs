namespace This_Is_War
{
    internal class Highscore
    {
        public string PlayerName { get; }
        public int Result { get; }

        public Highscore(string p, int r)
        {
            PlayerName = p;
            Result = r;
        }
    }
}