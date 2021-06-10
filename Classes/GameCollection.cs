using System;
using System.Collections.Generic;

namespace Classes
{
    class GameCollection
    {
        // todo: leave int ids after testing
        //  private Dictionary<int, GameManager> GameList;
         private Dictionary<string, GameManager> GameList = new Dictionary<string, GameManager>();

         public string CreateGame()
         {
            string testId = "Test";
            // int gameId = new Random().Next(0, 100000);
            
            // while (this.GameList.ContainsKey(testId))
            // {
            //     gameId = new Random().Next(0, 100000);
            // }

            if (!this.GameList.ContainsKey("Test"))
            {
                this.GameList[testId] = new GameManager();
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
            if (!this.GameList.ContainsKey(gameId))
            {
                return false;
            }

            this.GameList.Remove(gameId);

            return true;
        }

        public GameManager GetGame(string gameId)
        {
            if (!this.GameList.ContainsKey(gameId))
            {
                return null;
            }

            return this.GameList[gameId];
        }
    }
}