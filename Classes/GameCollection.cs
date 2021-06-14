using System;
using System.Collections.Generic;
using Dto;

namespace Classes
{
    class GameCollection
    {
        // todo: leave int ids after testing
        //  private Dictionary<int, GameManager> GameList;
         private Dictionary<string, GameManager> gameList = new Dictionary<string, GameManager>();

         public string CreateGame()
         {
            string testId = "Test";
            // int gameId = new Random().Next(0, 100000);
            
            // while (this.GameList.ContainsKey(testId))
            // {
            //     gameId = new Random().Next(0, 100000);
            // }

            if (!this.gameList.ContainsKey(testId))
            {
                this.gameList[testId] = new GameManager();
                this.gameList[testId].Players = new PlayerCollection(8);
            }

            // System.Console.WriteLine($"Game with the id {gameId} has been created!"); // maybe log it with a logger
            return testId;
         }

        // public bool RemoveGame(int gameId)
        // {
        //     if (!this.GameList.ContainsKey(gameId))
        //     {
        //         return false;
        //     }

        //     this.GameList.Remove(gameId);

        //     return true;
        // }

        // public GameManager GetGame(int gameId)
        // {
        //     if (!this.GameList.ContainsKey(gameId))
        //     {
        //         return null;
        //     }

        //     return this.GameList[gameId];
        // }

        public bool RemoveGame(string gameId)
        {
            if (!this.gameList.ContainsKey(gameId))
            {
                return false;
            }

            this.gameList.Remove(gameId);

            return true;
        }

        public GameManager GetGame(string gameId)
        {
            if (!this.gameList.ContainsKey(gameId))
            {
                return null;
            }

            return this.gameList[gameId];
        }


        public bool GameExists(string gameId)
        {
            return this.GetGame(gameId) != null;
        }
    }
}