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
        public int CorrectAnswers { get; set; }

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

        //todo: try to find a different way maybe?
        public void SetRoomAdmin()
        {
            Player adminPlayer = this.Players.GetPlayerAtPostion(0);
            if (adminPlayer != null && adminPlayer.IsAdmin == false)
            {
                adminPlayer.IsAdmin = true;
                this.DrawingPlayer = adminPlayer;
            }
        }

        public void ReSetAdmin(int removedUserId)
        {
            if (this.Players.GetPlayerById(removedUserId).IsAdmin)
            {
                if (this.GetNextPlayer() != null)
                {
                    this.GetNextPlayer().IsAdmin = true;
                }
            }
        }

        public bool IsFinished()
        {
            return this.Round > this.MaxRounds;
        }

        public bool IsCorrectWord(string word)
        {
            word = word.ToLower();

            return word == this.WordToGuess.ToLower();
        }

        public RoundInfo NextRound()
        {
            int currentPlayerId = this.DrawingPlayer.Id;
            string username = this.DrawingPlayer.Username;
            bool isLastRound = this.Round >= this.MaxRounds;
            Player nextPlayer = this.GetNextPlayer();

            this.Players.GetPlayerById(currentPlayerId).IsAdmin = false;
            nextPlayer.IsAdmin = true;
            this.DrawingPlayer = nextPlayer;

            RoundInfo roundInfo = new RoundInfo
            {
                Won = true,
                Username = username,
                IsLastRound = isLastRound,
                Round = this.Round
            };

            return roundInfo;
        }

        public void SetUpRound(string wordToGuess)
        {
            if (!this.InProgress)
            {
                this.MaxRounds = this.Players.PlayerCount * 2;
            }

            this.InProgress = true;
            this.WordToGuess = wordToGuess;
            this.Round++; // add check to see if max rounds is 0 because a player cannot play alone
            this.CorrectAnswers = 0;
            this.Players.SetGuessedCorretlyTo(false);
        }
    }
}