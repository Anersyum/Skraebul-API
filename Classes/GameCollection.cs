using System;
using System.Collections.Generic;
using Dto;

namespace Classes
{
    class GameCollection
    {
        // todo: leave int ids after testing
         private Dictionary<string, GameManager> gameList = new Dictionary<string, GameManager>();
        //  private Dictionary<string, GameManager> gameList = new Dictionary<string, GameManager>();

         public string CreateGame(string gameName = "")
         {
            if (gameName == "") 
            {
                gameName = "lsdkfjlksdj";
            }

            if (!this.gameList.ContainsKey(gameName))
            {
                this.gameList[gameName] = new GameManager();
                this.gameList[gameName].Players = new PlayerCollection(8);
            }

            // System.Console.WriteLine($"Game with the id {gameId} has been created!"); // maybe log it with a logger
            return gameName;
         }

        public bool RemoveGame(string gameName)
        {
            if (!this.gameList.ContainsKey(gameName))
            {
                return false;
            }

            this.gameList.Remove(gameName);

            return true;
        }

        public GameManager GetGame(string gameName)
        {
            if (!this.gameList.ContainsKey(gameName))
            {
                return null;
            }

            return this.gameList[gameName];
        }


        public bool GameExists(string gameName)
        {
            return this.GetGame(gameName) != null;
        }
    }
}