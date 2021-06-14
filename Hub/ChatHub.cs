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
        public override async Task OnConnectedAsync() 
        {
            string gameId = "Test";
            var username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                Context.Abort();
                return;
            }

            if (!GameCollection.GameExists(gameId))
            {
                GameCollection.CreateGame();
            }

            GameManager currentGame = GameCollection.GetGame(gameId);

            // todo: redesign the player ids and the particitpation
            int firstFreeId = 0;

            while (currentGame.Players.GetPlayerAtPostion(firstFreeId) != null)
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
            currentGame.Players.AddPlayer(player);
            currentGame.SetRoomAdmin();

            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            List<Player> activePlayers = currentGame.Players.ToList();

            await Clients.Group(gameId).SendAsync("Connected", activePlayers, username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception) 
        {
            GameManager currentGame = GameCollection.GetGame("Test");

            int userID = (int)Context.Items["UserID"];
            string username = currentGame.Players.GetPlayerById(userID).Username;

            currentGame.ReSetAdmin(userID);
            currentGame.Players.RemovePlayer(userID);
            
            // if there are no players, remove the game
            if (currentGame.Players.PlayerCount == 0)
            {
                GameCollection.RemoveGame("Test");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            List<Player> activePlayers = currentGame.Players.ToList();

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

            // cannot send word when there is less than one player in room
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

            if (currentGame.IsFinished())
            {
                return;
            }

            if (currentGame.IsCorrectWord(answer))
            {
                List<Player> activePlayers = new List<Player>();
                activePlayers = currentGame.Players.ToList();

                await Clients.Group("Test").SendAsync("RecieveAnswer", currentGame.NextRound(), activePlayers);
            }
        }
    }
}