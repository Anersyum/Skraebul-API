namespace Dto
{
    class GameManager
    {
        public string WordToGuess { get; set; }
        // public Dictionary<string, Player> Players { get; set; }
        public bool InProgress { get; set; }
        public Player DrawingPlayer { get; set; }
        public int Round { get; set; }
        public int MaxRounds { get; set; }
    }
}