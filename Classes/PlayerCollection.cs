using System.Collections.Generic;
using Skraebul_API.Dto;

namespace Skraebul_API.Classes;

internal class PlayerCollection
{
    public int PlayerCount { get; set; }
    
    public int MaxPlayerCount { get; set; }
    
    private Player[] _playerList;

    public PlayerCollection(int numberOfPlayers)
    {
        PlayerCount = 0;
        _playerList = new Player[numberOfPlayers];
        MaxPlayerCount = numberOfPlayers;
    }

    public bool AddPlayer(Player player)
    { 
        for (int i = 0; i < _playerList.Length; i++)
        {
            if (_playerList[i] == null)
            {
                _playerList[i] = player;
                PlayerCount++;
                return true;
            }
        }

        return false;
    }

    public bool RemovePlayer(Player player)
    {
        for (int i = 0; i < _playerList.Length; i++)
        {
            if (_playerList[i] != null && _playerList[i].Id == player.Id)
            {
                _playerList[i] = null;
                PlayerCount--;
                ShiftPlayers();
                return true;
            }
        }

        return false;
    }

    public bool RemovePlayer(int playerId)
    {
        for (int i = 0; i < _playerList.Length; i++)
        {
            if (_playerList[i] != null && _playerList[i].Id == playerId)
            {
                _playerList[i] = null;
                PlayerCount--;
                ShiftPlayers();
                return true;
            }
        }

        return false;
    }

    public List<Player> ToList()
    {
        List<Player> players = new List<Player>();

        for (int i = 0; i < MaxPlayerCount; i++)
        {
            if (_playerList[i] != null)
            {
                players.Add(_playerList[i]);
            }   
        }

        return players;
    }

    public Player GetPlayerAtPostion(int position)
    {
        if (position >= PlayerCount)
        {
            return null;
        }

        return _playerList[position];
    }

    public Player GetPlayerById(int playerId)
    {
        for (int i = 0; i < _playerList.Length; i++)
        {
            if (_playerList[i] != null && _playerList[i].Id == playerId)
            {
                return _playerList[i];
            }
        }

        return null;
    }

    public int GetPlayerPosition(Player player)
    {
        for (int i = 0; i < _playerList.Length; i++)
        {
            if (_playerList[i] != null && _playerList[i].Id == player.Id)
            {
                return i;
            }
        }

        return -1;
    }

    // todo: find a better way without shifting. Maybe with collections?
    private void ShiftPlayers()
    {
        Player[] sortedPlayers = new Player[MaxPlayerCount];
        int filledPositions = 0;

        for (int i = 0; i < MaxPlayerCount; i++)
        {
            if (_playerList[i] != null)
            {
                sortedPlayers[filledPositions] = _playerList[i];
                filledPositions++;
            }
        }

        _playerList = sortedPlayers;
        sortedPlayers = null;
    }

    public void SetGuessedCorretlyTo(bool guessedCorrectly)
    {
        for (int i = 0; i < _playerList.Length; i++)
        {
            if (_playerList[i] != null)
            {
                _playerList[i].GuessedCorrectly = guessedCorrectly;
            }
        }
    }
}