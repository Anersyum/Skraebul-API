using Skraebul_API.Dto;

namespace Skraebul_API.Classes;

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
        int currentDrawingPlayerPosition = Players.GetPlayerPosition(DrawingPlayer);
        Player nextPlayer = Players.GetPlayerAtPostion(currentDrawingPlayerPosition + 1);

        if (nextPlayer == null)
        {
            return Players.GetPlayerAtPostion(0);
        }

        return nextPlayer;
    }

    //todo: try to find a different way maybe?
    public void SetRoomAdmin()
    {
        Player adminPlayer = Players.GetPlayerAtPostion(0);
        if (adminPlayer != null && adminPlayer.IsAdmin == false)
        {
            adminPlayer.IsAdmin = true;
            DrawingPlayer = adminPlayer;
        }
    }

    public void ReSetAdmin(int removedUserId)
    {
        if (Players.GetPlayerById(removedUserId).IsAdmin)
        {
            if (GetNextPlayer() != null)
            {
                GetNextPlayer().IsAdmin = true;
            }
        }
    }

    public bool IsFinished()
    {
        return Round > MaxRounds;
    }

    public bool IsCorrectWord(string word)
    {
        if (WordToGuess == null) {
            return false;
        }

        word = word.ToLower();

        return word == WordToGuess.ToLower();
    }

    public RoundInfo NextRound()
    {
        int currentPlayerId = DrawingPlayer.Id;
        string username = DrawingPlayer.Username;
        bool isLastRound = Round >= MaxRounds;
        Player nextPlayer = GetNextPlayer();

        Players.GetPlayerById(currentPlayerId).IsAdmin = false;
        nextPlayer.IsAdmin = true;
        DrawingPlayer = nextPlayer;

        RoundInfo roundInfo = new RoundInfo
        {
            Won = true,
            Username = username,
            IsLastRound = isLastRound,
            Round = Round
        };

        return roundInfo;
    }

    public void SetUpRound(string wordToGuess)
    {
        if (!InProgress)
        {
            MaxRounds = Players.PlayerCount * 2;
        }

        InProgress = true;
        WordToGuess = wordToGuess;
        Round++; // add check to see if max rounds is 0 because a player cannot play alone
        CorrectAnswers = 0;
        Players.SetGuessedCorretlyTo(false);
    }

    public bool IsRoomFull()
    {
        return NumberOfPlayers >= 8;
    }
}