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
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();
            bool isJoiningRoom = Convert.ToBoolean(Context.GetHttpContext().Request.Query["joinroom"].ToString());
            var username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                Context.Abort();
                return;
            }

            if (!GameCollection.GameExists(gameId) && !isJoiningRoom)
            {
                GameCollection.CreateGame(gameId);
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
                IsAdmin = false,
                GuessedCorrectly = false
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
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();
            GameManager currentGame = GameCollection.GetGame(gameId);

            int userID = (int)Context.Items["UserID"];
            string username = currentGame.Players.GetPlayerById(userID).Username;

            currentGame.ReSetAdmin(userID);
            currentGame.Players.RemovePlayer(userID);
            
            // if there are no players, remove the game
            if (currentGame.Players.PlayerCount == 0)
            {
                GameCollection.RemoveGame(gameId);
                await base.OnDisconnectedAsync(exception);
                return;
            }

            List<Player> activePlayers = currentGame.Players.ToList();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            await Clients.OthersInGroup(gameId).SendAsync("Disconnected", activePlayers, username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMove(Move move) 
        {
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();

            await Clients.OthersInGroup(gameId).SendAsync("RecieveMove", move);
        }

        public async Task SendChosenWord(string word) 
        {
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();
            GameManager currentGame = GameCollection.GetGame(gameId);

            // cannot send word when there is less than one player in room
            if (currentGame.Players.PlayerCount <= 1)
            {
                return;
            }

            currentGame.SetUpRound(word);

            await Clients.OthersInGroup(gameId).SendAsync("RecieveChosenWord", word);
        }

        public async Task SendUncoveredLetter(string letter, int letterPosition)
        {
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();

            await Clients.OthersInGroup(gameId).SendAsync("RecieveUncoveredLetter", letter, letterPosition);
        }

        public async Task SendAnswer(string answer, int time)
        {
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();
            GameManager currentGame = GameCollection.GetGame(gameId);
            
            if (currentGame.IsFinished())
            {
                return;
            }

            if (currentGame.IsCorrectWord(answer))
            {
                int playerId = (int)Context.Items["UserID"];
                Player guessingPlayer = currentGame.Players.GetPlayerById(playerId);

                if (!guessingPlayer.GuessedCorrectly)
                {
                    currentGame.CorrectAnswers++;
                    guessingPlayer.GuessedCorrectly = true;
                    guessingPlayer.Points += (time * 2);
                    await Clients.Caller.SendAsync("RecieveAnswerMessage", answer);
                }

                if (currentGame.CorrectAnswers >= (currentGame.Players.PlayerCount - 1))
                {
                    List<Player> activePlayers = new List<Player>();
                    activePlayers = currentGame.Players.ToList();

                    await Clients.Group(gameId).SendAsync("RecieveAnswer", currentGame.NextRound(), activePlayers);
                }
            }
        }

        public async Task EndRoundViaTimer()
        {
            string gameId = Context.GetHttpContext().Request.Query["room"].ToString();
            GameManager currentGame = GameCollection.GetGame(gameId);

            List<Player> activePlayers = new List<Player>();
            activePlayers = currentGame.Players.ToList();

            await Clients.Group(gameId).SendAsync("EndRoundViaTimer", currentGame.NextRound(), activePlayers);
        }
    }
}