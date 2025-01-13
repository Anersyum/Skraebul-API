using System.Collections.Generic;

namespace Skraebul_API.Classes;

internal class GameCollection
{
    // todo: leave int ids after testing
    private readonly Dictionary<string, GameManager> _gameList = new();
    //  private Dictionary<string, GameManager> gameList = new Dictionary<string, GameManager>();

    public string CreateGame(string gameName = "")
    {
        if (gameName == "") 
        {
            gameName = "lsdkfjlksdj";
        }

        if (!_gameList.ContainsKey(gameName))
        {
            _gameList[gameName] = new GameManager();
            _gameList[gameName].Players = new PlayerCollection(8);
        }

        // System.Console.WriteLine($"Game with the id {gameId} has been created!"); // maybe log it with a logger
        return gameName;
    }

    public bool RemoveGame(string gameName)
    {
        if (!_gameList.ContainsKey(gameName))
        {
            return false;
        }

        _gameList.Remove(gameName);

        return true;
    }

    public GameManager GetGame(string gameName)
    {
        if (!_gameList.ContainsKey(gameName))
        {
            return null;
        }

        return _gameList[gameName];
    }


    public bool GameExists(string gameName)
    {
        return GetGame(gameName) != null;
    }
}