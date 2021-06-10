using Dto;

namespace Classes
{
    class GameManager
    {
        public string WordToGuess { get; set; }
        public PlayerCollection Players { get; set; }
        public bool InProgress { get; set; }
        public Player DrawingPlayer { get; set; }
        public int Round { get; set; }
        public int MaxRounds { get; set; }
        public int NumberOfPlayers { get; set; }

        public Player GetNextPlayer()
        {
            int currentDrawingPlayerPositon = Players.GetPlayerPosition(DrawingPlayer);
            Player nextPlayer = Players.GetPlayerAtPostion(currentDrawingPlayerPositon + 1);

            if (nextPlayer == null)
            {
                return Players.GetPlayerAtPostion(0);
            }

            return nextPlayer;
        }

    }
}