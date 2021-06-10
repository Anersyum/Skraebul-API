using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Dto;
using Classes;

namespace Hubs
{
    class ChatHub : Hub
    {
        private static GameCollection GameCollection = new GameCollection();

        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync("RecieveMessage", new {username = username, message = message});
        }

        // todo: generate random room ids for groups
        // todo: add players to game object
        public override async Task OnConnectedAsync() 
        {
            string gameId = "";
            var username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "Test");

            if (GameCollection.GetGame("Test") == null) 
            {
                gameId = GameCollection.CreateGame();
                GameCollection.GetGame("Test").Players = new PlayerCollection(8);
            }

            // todo: redesign the player ids and the particitpation
            int firstFreeId = 0;

            while (GameCollection.GetGame("Test").Players.GetPlayerAtPostion(firstFreeId) != null)
            {
                firstFreeId++;
            }

            Player player = new Player
            {
                Id = firstFreeId,
                Username = username,
                Points = 0,
                IsAdmin = false
            };
            
            Context.Items.Add("UserID", player.Id);

            if (GameCollection.GetGame("Test").Players.PlayerCount < 1) 
            {
                player.IsAdmin = true;
                GameCollection.GetGame("Test").DrawingPlayer = player;
            }

            GameCollection.GetGame("Test").Players.AddPlayer(player);

            List<Player> activePlayers = new List<Player>();

            for (int i = 0; i < GameCollection.GetGame("Test").Players.MaxPlayerCount; i++)
            {
                if (GameCollection.GetGame("Test").Players.GetPlayerAtPostion(i) != null)
                {
                    activePlayers.Add(GameCollection.GetGame("Test").Players.GetPlayerAtPostion(i));
                }   
            }

            await Clients.Group("Test").SendAsync("Connected", activePlayers, username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception) 
        {
            GameManager currentGame = GameCollection.GetGame("Test");

            int userID = (int)Context.Items["UserID"];
            string username = GameCollection.GetGame("Test").Players.GetPlayerById(userID).Username;

            if (currentGame.Players.GetPlayerById(userID).IsAdmin)
            {
                if (currentGame.GetNextPlayer() != null)
                {
                    currentGame.GetNextPlayer().IsAdmin = true;
                }
            }

            currentGame.Players.RemovePlayer(userID);
            
            if (currentGame.Players.PlayerCount == 0)
            {
                GameCollection.RemoveGame("Test");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            List<Player> activePlayers = new List<Player>();
            
            for (int i = 0; i < currentGame.Players.MaxPlayerCount; i++)
            {
                if (currentGame.Players.GetPlayerAtPostion(i) != null)
                {
                    activePlayers.Add(currentGame.Players.GetPlayerAtPostion(i));
                }   
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Test");
            await Clients.OthersInGroup("Test").SendAsync("Disconnected", activePlayers, username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMove(Move move) 
        {
            await Clients.OthersInGroup("Test").SendAsync("RecieveMove", move);
        }

        public async Task SendChosenWord(string word) 
        {
            GameManager currentGame = GameCollection.GetGame("Test");

            if (currentGame.Players.PlayerCount <= 1)
            {
                return;
            }
            currentGame.WordToGuess = word;
            currentGame.MaxRounds = currentGame.Players.PlayerCount * 2;
            currentGame.Round++; // add check to see if max rounds is 0 because a player cannot play alone

            await Clients.OthersInGroup("Test").SendAsync("RecieveChosenWord", word);
        }

        public async Task SendUncoveredLetter(string letter, int letterPosition)
        {
            await Clients.OthersInGroup("Test").SendAsync("RecieveUncoveredLetter", letter, letterPosition);
        }

        public async Task SendAnswer(string answer)
        {
            GameManager currentGame = GameCollection.GetGame("Test");
            // todo: gamemanager should decide the wining condition
            if (currentGame.Round > currentGame.MaxRounds)
            {
                return;
            }

            if (currentGame.WordToGuess == answer)
            {
                int currentPlayerId = currentGame.DrawingPlayer.Id;
                string username = currentGame.DrawingPlayer.Username;
                bool isLastRound = false;

                if (currentGame.Round >= currentGame.MaxRounds)
                {
                    isLastRound = true;
                }

                Player nextPlayer = currentGame.GetNextPlayer();
                List<Player> activePlayers = new List<Player>();

                currentGame.Players.GetPlayerById(currentPlayerId).IsAdmin = false;
                nextPlayer.IsAdmin = true;
                currentGame.DrawingPlayer = nextPlayer;

                for (int i = 0; i < currentGame.Players.MaxPlayerCount; i++)
                {
                    if (currentGame.Players.GetPlayerAtPostion(i) != null)
                    {
                        activePlayers.Add(currentGame.Players.GetPlayerAtPostion(i));
                    }   
                }

                // add a object instead of the anon object
                await Clients.Group("Test").SendAsync("RecieveAnswer", new { won = true, username = username, lastRound = isLastRound, round = GameCollection.GetGame("Test").Round }, activePlayers);
            }
        }
    }
}