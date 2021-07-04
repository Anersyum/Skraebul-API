using System;
using System.Collections.Generic;
using Dto;

namespace Classes
{
    class GameCollection
    {
        // todo: leave int ids after testing
         private Dictionary<ulong, GameManager> gameList;
         private ulong gameNumber = 0;
        //  private Dictionary<string, GameManager> gameList = new Dictionary<string, GameManager>();

         public ulong CreateGame()
         {
            ++gameNumber;
            ulong gameId = gameNumber;

            if (!this.gameList.ContainsKey(gameId))
            {
                this.gameList[gameId] = new GameManager();
                this.gameList[gameId].Players = new PlayerCollection(8);
            }

            // System.Console.WriteLine($"Game with the id {gameId} has been created!"); // maybe log it with a logger
            return gameId;
         }

        public bool RemoveGame(ulong gameId)
        {
            if (!this.gameList.ContainsKey(gameId))
            {
                return false;
            }

            this.gameList.Remove(gameId);

            return true;
        }

        public GameManager GetGame(ulong gameId)
        {
            if (!this.gameList.ContainsKey(gameId))
            {
                return null;
            }

            return this.gameList[gameId];
        }


        public bool GameExists(ulong gameId)
        {
            return this.GetGame(gameId) != null;
        }
    }
}