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
        private static Dictionary<string, Dictionary<string, Player>> Users = new Dictionary<string, Dictionary<string, Player>>();

        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync("RecieveMessage", new {username = username, message = message});
        }

        // todo: generate random room ids for groups
        public override async Task OnConnectedAsync() 
        {
            var username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "Test");

            Player player = new Player
            {
                Username = username,
                Points = 0,
                IsAdmin = false
            };

            if (!Users.ContainsKey("Test")) 
            {
                Users["Test"] = new Dictionary<string, Player>();
            }
            
            Users["Test"].Add(Context.ConnectionId, player);

            if (Users["Test"].Count <= 1) 
            {
                player.IsAdmin = true;
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
            string username = Users["Test"][Context.ConnectionId].Username;
            Users["Test"].Remove(Context.ConnectionId);
            
            List<Player> activePlayers = new List<Player>();

            foreach (KeyValuePair<string, Player> kvp in Users["Test"])
            {
                activePlayers.Add(kvp.Value);
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Test");
            await Clients.Group("Test").SendAsync("Disconnected", activePlayers, username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMove(Position position) 
        {
            await Clients.OthersInGroup("Test").SendAsync("RecieveMove", position);
        }

        public async Task StartGame() {
            await Clients.OthersInGroup("Test").SendAsync("RecieveGameStarted");
        }
    }
}