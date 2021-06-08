using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Dto;

namespace Hubs
{
    class ChatHub : Hub
    {
        // try making a dictinoary on the player model so that everything can go under the game dictionary
        private static Dictionary<string, Dictionary<string, Player>> Users = new Dictionary<string, Dictionary<string, Player>>();
        private static Dictionary<string, GameManager> Game = new Dictionary<string, GameManager>();

        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync("RecieveMessage", new {username = username, message = message});
        }

        // todo: generate random room ids for groups
        // todo: add players to game object
        public override async Task OnConnectedAsync() 
        {
            var username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "Test");

            if (!Users.ContainsKey("Test")) 
            {
                Users["Test"] = new Dictionary<string, Player>();
                Game["Test"] = new GameManager() 
                {
                    WordToGuess = "",
                    DrawingPlayer = null,
                    MaxRounds = 0,
                    Round = 0,
                    InProgress = false,
                    NumberOfPlayers = 0
                };
            }

            // todo: redesign the player ids and the particitpation
            int firstFreeId = 0;

            while (Users["Test"].ContainsKey(firstFreeId.ToString()))
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
            Users["Test"].Add(player.Id.ToString(), player);

            if (Users["Test"].Count <= 1) 
            {
                player.IsAdmin = true;
                Game["Test"].DrawingPlayer = player;
            }

            List<Player> activePlayers = new List<Player>();

            foreach (KeyValuePair<string, Player> kvp in Users["Test"])
            {
                activePlayers.Add(kvp.Value);
            }

            await Clients.Group("Test").SendAsync("Connected", activePlayers, username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception) 
        {
            string userID = Context.Items["UserID"].ToString();
            string username = Users["Test"][userID].Username;

            Users["Test"].Remove(userID);
            
            if (Users["Test"].Count == 0)
            {
                Users.Remove("Test");
                Game.Remove("Test");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            List<Player> activePlayers = new List<Player>();
            int count = 0;
            foreach (KeyValuePair<string, Player> kvp in Users["Test"])
            {
                if (count < 1)
                {
                    kvp.Value.IsAdmin = true;
                    count++;
                }

                activePlayers.Add(kvp.Value);
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
            if (Users["Test"].Count <= 1)
            {
                return;
            }
            Game["Test"].WordToGuess = word;
            Game["Test"].MaxRounds = Users["Test"].Count * 2;
            Game["Test"].Round++; // add check to see if max rounds is 0 because a player cannot play alone

            await Clients.OthersInGroup("Test").SendAsync("RecieveChosenWord", word);
        }

        public async Task SendUncoveredLetter(string letter, int letterPosition)
        {
            await Clients.OthersInGroup("Test").SendAsync("RecieveUncoveredLetter", letter, letterPosition);
        }

        public async Task SendAnswer(string answer)
        {
            if (Game["Test"].Round > Game["Test"].MaxRounds)
            {
                return;
            }

            if (Game["Test"].WordToGuess == answer)
            {
                int currentPlayerId = Game["Test"].DrawingPlayer.Id;
                string username = Users["Test"][currentPlayerId.ToString()].Username;
                bool isLastRound = false;

                if (Game["Test"].Round >= Game["Test"].MaxRounds)
                {
                    isLastRound = true;
                }

                int nextPlayerId = currentPlayerId + 1;
                List<Player> activePlayers = new List<Player>();

                while (nextPlayerId < 8)
                {
                    if (Users["Test"].ContainsKey(nextPlayerId.ToString()))
                    {
                        Users["Test"][currentPlayerId.ToString()].IsAdmin = false;
                        Users["Test"][nextPlayerId.ToString()].IsAdmin = true;
                        Game["Test"].DrawingPlayer = Users["Test"][nextPlayerId.ToString()];

                        foreach (KeyValuePair<string, Player> kvp in Users["Test"])
                        {
                            activePlayers.Add(kvp.Value);
                        }
                        break;
                    }
                    nextPlayerId++;
                }

                if (Game["Test"].DrawingPlayer.Id == Users["Test"][currentPlayerId.ToString()].Id)
                {
                    int count = 0;
                    while (count < 8)
                    {
                        if (Users["Test"].ContainsKey(count.ToString()))
                        {
                            Users["Test"][currentPlayerId.ToString()].IsAdmin = false;
                            Users["Test"][count.ToString()].IsAdmin = true;
                            Game["Test"].DrawingPlayer = Users["Test"][count.ToString()];

                            foreach (KeyValuePair<string, Player> kvp in Users["Test"])
                            {
                                activePlayers.Add(kvp.Value);
                            }
                            break;
                        }
                        count++;
                    }
                }
                // Game["Test"].Round++;

                // add a object instead of the anon object
                await Clients.Group("Test").SendAsync("RecieveAnswer", new { won = true, username = username, lastRound = isLastRound, round = Game["Test"].Round }, activePlayers);
            }
        }
    }
}