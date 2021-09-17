using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Dto;
using Classes;
using System.Text.RegularExpressions;

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
            // check for empty room number
            string roomName = Context.GetHttpContext().Request.Query["room"].ToString();

            bool isJoiningRoom = Convert.ToBoolean(Context.GetHttpContext().Request.Query["joinroom"].ToString());
            string username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                await Clients.Caller.SendAsync("FailedToConnect", "Username can't be empty.");
                Context.Abort();
                return;
            }

            if (!GameCollection.GameExists(roomName))
            {
                if (!isJoiningRoom)
                {
                   roomName = GameCollection.CreateGame(roomName);
                }
                else 
                {
                    await Clients.Caller.SendAsync("FailedToConnect", "The room does not exist.");
                    Context.Abort();
                    return;
                }
            }

            GameManager currentGame = GameCollection.GetGame(roomName);

            if (currentGame.InProgress)
            {
                await Clients.Caller.SendAsync("FailedToConnect", "This game has already started.");
                Context.Abort();
                return;
            }

            if (currentGame.IsRoomFull())
            {
                await Clients.Caller.SendAsync("FailedToConnect", "The room is full.");
                Context.Abort();
                return;
            }
            
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
            Context.Items.Add("GameID", roomName);
            currentGame.Players.AddPlayer(player);
            currentGame.SetRoomAdmin();

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            List<Player> activePlayers = currentGame.Players.ToList();

            await Clients.Group(roomName).SendAsync("Connected", activePlayers, username, roomName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception) 
        {
            if (!Context.Items.ContainsKey("GameID"))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            string roomName = (string)Context.Items["GameID"];
            
            if (!GameCollection.GameExists(roomName)) 
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            GameManager currentGame = GameCollection.GetGame(roomName);

            int userID = (int)Context.Items["UserID"];
            string username = currentGame.Players.GetPlayerById(userID).Username;

            currentGame.ReSetAdmin(userID);
            currentGame.Players.RemovePlayer(userID);
            
            // if there are no players, remove the game
            if (currentGame.Players.PlayerCount == 0)
            {
                GameCollection.RemoveGame(roomName);
                await base.OnDisconnectedAsync(exception);
                return;
            }

            List<Player> activePlayers = currentGame.Players.ToList();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName.ToString());
            await Clients.OthersInGroup(roomName.ToString()).SendAsync("Disconnected", activePlayers, username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMove(Move move) 
        {
            string roomName = (string)Context.Items["GameID"];

            await Clients.OthersInGroup(roomName).SendAsync("RecieveMove", move);
        }

        public async Task SendChosenWord(string word) 
        {
            string roomName = (string)Context.Items["GameID"];
            GameManager currentGame = GameCollection.GetGame(roomName);

            // cannot send word when there is less than one player in room
            if (currentGame.Players.PlayerCount <= 1)
            {
                return;
            }

            currentGame.SetUpRound(word);

            await Clients.OthersInGroup(roomName).SendAsync("RecieveChosenWord", word);
        }

        public async Task SendUncoveredLetter(string letter, int letterPosition)
        {
            string roomName = (string)Context.Items["GameID"];

            await Clients.OthersInGroup(roomName).SendAsync("RecieveUncoveredLetter", letter, letterPosition);
        }

        public async Task SendAnswer(string answer, int time)
        {
            string roomName = (string)Context.Items["GameID"];
            GameManager currentGame = GameCollection.GetGame(roomName);
            
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
                    guessingPlayer.GottenPoints = time * 2;
                    await Clients.Caller.SendAsync("RecieveAnswerMessage", answer);
                }

                if (currentGame.CorrectAnswers >= (currentGame.Players.PlayerCount - 1))
                {
                    List<Player> activePlayers = new List<Player>();
                    activePlayers = currentGame.Players.ToList();

                    await Clients.Group(roomName).SendAsync("RecieveAnswer", currentGame.NextRound(), activePlayers);
                }
            }
        }

        public async Task EndRoundViaTimer()
        {
            string roomName = (string)Context.Items["GameID"];
            GameManager currentGame = GameCollection.GetGame(roomName);

            List<Player> activePlayers = new List<Player>();
            activePlayers = currentGame.Players.ToList();

            await Clients.Group(roomName).SendAsync("EndRoundViaTimer", currentGame.NextRound(), activePlayers);
        }
    }
}