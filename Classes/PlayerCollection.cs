using System.Collections.Generic;
using Dto;

namespace Classes
{
    class PlayerCollection
    {
        public int PlayerCount { get; set; }
        public int MaxPlayerCount { get; set; }
        private Player[] playerList;

        public PlayerCollection(int numberOfPlayers)
        {
            this.PlayerCount = 0;
            this.playerList = new Player[numberOfPlayers];
            this.MaxPlayerCount = numberOfPlayers;
        }

        public bool AddPlayer(Player player)
        { 
            for (int i = 0; i < this.playerList.Length; i++)
            {
                if (this.playerList[i] == null)
                {
                    this.playerList[i] = player;
                    this.PlayerCount++;
                    return true;
                }
            }

            return false;
        }

        public bool RemovePlayer(Player player)
        {
            for (int i = 0; i < this.playerList.Length; i++)
            {
                if (this.playerList[i] != null && this.playerList[i].Id == player.Id)
                {
                    this.playerList[i] = null;
                    this.PlayerCount--;
                    this.ShiftPlayers();
                    return true;
                }
            }

            return false;
        }

        public bool RemovePlayer(int playerId)
        {
            for (int i = 0; i < this.playerList.Length; i++)
            {
                if (this.playerList[i] != null && this.playerList[i].Id == playerId)
                {
                    this.playerList[i] = null;
                    this.PlayerCount--;
                    this.ShiftPlayers();
                    return true;
                }
            }

            return false;
        }

        public List<Player> ToList()
        {
            List<Player> players = new List<Player>();

            for (int i = 0; i < this.MaxPlayerCount; i++)
            {
                if (this.playerList[i] != null)
                {
                    players.Add(this.playerList[i]);
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

            return this.playerList[position];
        }

        public Player GetPlayerById(int playerId)
        {
            for (int i = 0; i < this.playerList.Length; i++)
            {
                if (this.playerList[i] != null && this.playerList[i].Id == playerId)
                {
                    return this.playerList[i];
                }
            }

            return null;
        }

        public int GetPlayerPosition(Player player)
        {
            for (int i = 0; i < this.playerList.Length; i++)
            {
                if (this.playerList[i] != null && this.playerList[i].Id == player.Id)
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
                if (this.playerList[i] != null)
                {
                    sortedPlayers[filledPositions] = this.playerList[i];
                    filledPositions++;
                }
            }

            this.playerList = sortedPlayers;
            sortedPlayers = null;
        }
    }
}